using System;
using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Model.XConnect;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.Framework.Conditions;
using Sitecore.Globalization;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Base
{
    public abstract class SaveMarketingPreferencesBase<T> : SubmitActionBase<T> where T : SaveMarketingPreferencesData
    {
        [NonSerialized]
        protected new readonly ILogger Logger;
        [NonSerialized]
        private readonly IXConnectContactService _xConnectContactService;
        [NonSerialized]
        private readonly IXConnectContactFactory _xConnectContactFactory;
        [NonSerialized]
        private readonly ISaveMarketingPreferencesService<T> _saveMarketingPreferencesService;
        [NonSerialized]
        private readonly IMarketingPreferencesService _marketingPreferenceService;
        [NonSerialized]
        private readonly IExmSubscriptionClientApiService _exmSubscriptionClientApiService;
        private readonly bool _useDoubleOptIn = Settings.GetBoolSetting("NewsletterSubscription.UseDoubleOptInForSubscription", true); // GDPR, sorry for the default value!

        protected SaveMarketingPreferencesBase(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectContactService xConnectContactService,
            IXConnectContactFactory xConnectContactFactory,
            ISaveMarketingPreferencesService<T> saveMarketingPreferencesService,
            IMarketingPreferencesService marketingPreferenceService,
            IExmSubscriptionClientApiService exmSubscriptionClientApiService)
            : base(submitActionData)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            Condition.Requires(xConnectContactFactory, nameof(xConnectContactFactory)).IsNotNull();
            Condition.Requires(saveMarketingPreferencesService, nameof(saveMarketingPreferencesService)).IsNotNull();
            Condition.Requires(marketingPreferenceService, nameof(marketingPreferenceService)).IsNotNull();
            Condition.Requires(exmSubscriptionClientApiService, nameof(exmSubscriptionClientApiService)).IsNotNull();

            Logger = logger;
            _xConnectContactService = xConnectContactService;
            _xConnectContactFactory = xConnectContactFactory;
            _saveMarketingPreferencesService = saveMarketingPreferencesService;
            _marketingPreferenceService = marketingPreferenceService;
            _exmSubscriptionClientApiService = exmSubscriptionClientApiService;
        }

        protected override bool Execute(T data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, nameof(formSubmitContext));

            if (data.FieldEmailAddressId == Guid.Empty)
            {
                Logger.LogError("Field email address couldn't be empty!");
                return false;
            }

            try
            {
                if (!formSubmitContext.HasErrors)
                {
                    ProcessContact(data, formSubmitContext.Fields);
                }
                else
                {
                    Logger.LogWarn($"Form {formSubmitContext.FormId} submitted with errors: {string.Join(", ", formSubmitContext.Errors.Select(t => t.ErrorMessage))}.");
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message, ex);
                return false;
            }
        }

        private void ProcessContact(T data, IList<IViewModel> fields)
        {
            var contactIdentifier = GetContactIdentifier(data, fields);
            if (contactIdentifier == null)
            {
                throw new ContactIdentifierException();
            }

            var marketingPreferencesViewModel = _saveMarketingPreferencesService.GetMarketingPreferencesViewModel(fields);
            if (marketingPreferencesViewModel == null)
            {
                throw new MarketingPreferencesException();
            }

            var managerRoot = _saveMarketingPreferencesService.GetManagerRoot(marketingPreferencesViewModel);
            if (managerRoot == null)
            {
                throw new ManagerRootException();
            }

            var listId = ParseContactListId(marketingPreferencesViewModel);

            if (!listId.HasValue)
            {
                throw new ContactListException();
            }

            var contact = GetXConnectContactByIdentifer(contactIdentifier);
            if (contact != null)
            {
                if (IsContactSubscribedToList(contact, listId.Value))
                {
                    if (!_saveMarketingPreferencesService.AuthenticateContact(contact))
                    {
                        throw new ContactIdentifierException();
                    }

                    var marketingPreferences = _saveMarketingPreferencesService.GetSelectedMarketingPreferences(marketingPreferencesViewModel, managerRoot, contact.ExmKeyBehaviorCache()?.MarketingPreferences).ToList();
                    _marketingPreferenceService.SavePreferences(contact, marketingPreferences);
                    return;
                }
            }

            var customXConnectContact = _xConnectContactFactory.CreateContactWithEmail(contactIdentifier.Identifier);
            _xConnectContactService.IdentifyCurrent(customXConnectContact);
            _xConnectContactService.UpdateOrCreateContact(customXConnectContact);

            var newContact = GetXConnectContactByIdentifer(contactIdentifier);
            if (newContact == null)
            {
                throw new ContactException();
            }

            _saveMarketingPreferencesService.SetPersonalInformation(data, fields, newContact, contactIdentifier);
            _saveMarketingPreferencesService.SetExmKeyBehaviorCache(newContact);

            var newMarketingPreferences = _saveMarketingPreferencesService.GetSelectedMarketingPreferences(marketingPreferencesViewModel, managerRoot, newContact.ExmKeyBehaviorCache()?.MarketingPreferences).ToList();

            if (newMarketingPreferences.All(x => x.Preference == false))
            {
                UnsubscribeFromAll(contactIdentifier, managerRoot);
                return;
            }

            SubscribeContact(managerRoot, listId.Value, contactIdentifier, newMarketingPreferences);
        }

        private static Guid? ParseContactListId(MarketingPreferencesViewModel marketingPreferencesViewModel)
        {
            if (!Guid.TryParse(marketingPreferencesViewModel.ContactListId, out var parsedListId) || parsedListId == Guid.Empty)
            {
                return null;
            }

            return parsedListId;
        }

        private static bool IsContactSubscribedToList(Contact contact, Guid listId)
        {
            var listSubscription = contact.ListSubscriptions();
            if (listSubscription == null)
            {
                return false;
            }
            return listSubscription.Subscriptions.Any(x => {
                if (!x.IsActive)
                {
                    return false;
                }
                return x.ListDefinitionId == listId;
            });
        }


        private void UnsubscribeFromAll(ContactIdentifier contactIdentifier, ManagerRoot managerRoot)
        {
            _saveMarketingPreferencesService.ResetExmKeyBehaviorCache(contactIdentifier);
            _exmSubscriptionClientApiService.UnsubscribeFromAll(contactIdentifier, managerRoot);
        }

        private void SubscribeContact(ManagerRoot managerRoot, Guid listId, ContactIdentifier contactIdentifier, List<MarketingPreference> marketingPreferences)
        {
            if (_useDoubleOptIn)
            {
                // If DoubleOptIn should be used the contact is subscribed to the selected contact list and gets a confirmation mail
                // The contact is only subscribed to the global contact list, which is used as source for the segmented lists, if he confirms the link in the mail
                var message = new SubscribeContactMessage
                {
                    RecipientListId = listId,
                    ContactIdentifier = contactIdentifier,
                    ManagerRootId = managerRoot.Id,
                    SendSubscriptionConfirmation = true,
                    ContextLanguage = Language.Current
                };
                _exmSubscriptionClientApiService.Subscribe(message);
            }

            var contact = GetXConnectContactByIdentifer(contactIdentifier);
            _marketingPreferenceService.SavePreferences(contact, marketingPreferences);
        }

        protected abstract ContactIdentifier GetContactIdentifier(T data, IEnumerable<IViewModel> fields);
        private Contact GetXConnectContactByIdentifer(ContactIdentifier contactIdentifier)
        {
            return _xConnectContactService.GetXConnectContact(contactIdentifier, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);
        }
    }
}
