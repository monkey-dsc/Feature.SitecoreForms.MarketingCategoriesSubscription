using System;
using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Cd.Services;
using Sitecore.EmailCampaign.Model.XConnect;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;
using Sitecore.Framework.Conditions;
using Sitecore.ListManagement;
using Sitecore.Marketing.Definitions.ContactLists;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Base
{
    public abstract class SaveMarketingPreferencesBase<T> : SubmitActionBase<T> where T : SaveMarketingPreferencesData
    {
        protected new readonly ILogger Logger;
        private readonly IXConnectContactService _xConnectContactService;
        private readonly IXConnectContactFactory _xConnectContactFactory;
        private readonly ISaveMarketingPreferencesService<T> _saveMarketingPreferencesService;
        private readonly IMarketingPreferencesService _marketingPreferenceService;
        private readonly IExmSubscriptionManager _exmSubscriptionManager;
        private readonly ListManagerWrapper _listManagerWrapper;
        private readonly bool _useDoubleOptIn = Settings.GetBoolSetting("NewsletterSubscription.UseDoubleOptInForSubscription", true); // GDPR, sorry for the default value!

        protected SaveMarketingPreferencesBase(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectContactService xConnectContactService,
            IXConnectContactFactory xConnectContactFactory,
            ISaveMarketingPreferencesService<T> saveMarketingPreferencesService,
            IMarketingPreferencesService marketingPreferenceService,
            IExmSubscriptionManager exmSubscriptionManager,
            ListManagerWrapper listManagerWrapper) : base(submitActionData)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            Condition.Requires(xConnectContactFactory, nameof(xConnectContactFactory)).IsNotNull();
            Condition.Requires(saveMarketingPreferencesService, nameof(saveMarketingPreferencesService)).IsNotNull();
            Condition.Requires(marketingPreferenceService, nameof(marketingPreferenceService)).IsNotNull();
            Condition.Requires(exmSubscriptionManager, nameof(exmSubscriptionManager)).IsNotNull();
            Condition.Requires(exmSubscriptionManager, nameof(exmSubscriptionManager)).IsNotNull();

            Logger = logger;
            _xConnectContactService = xConnectContactService;
            _xConnectContactFactory = xConnectContactFactory;
            _saveMarketingPreferencesService = saveMarketingPreferencesService;
            _marketingPreferenceService = marketingPreferenceService;
            _exmSubscriptionManager = exmSubscriptionManager;
            _listManagerWrapper = listManagerWrapper;
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

            var contact = _xConnectContactService.GetXConnectContact(contactIdentifier, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);
            if (contact != null)
            {
                var globalList = _listManagerWrapper.FindById(new Guid(marketingPreferencesViewModel.ContactListId));
                if (globalList != null)
                {
                    var contacts = _listManagerWrapper.GetContacts(globalList);
                    if (contacts.Any(x => x.Identifiers
                                           .Where(i => i.IdentifierType == ContactIdentifierType.Known)
                                           .Any(y => y.Identifier == contactIdentifier.Identifier)))
                    {
                        var marketingPreferences = _saveMarketingPreferencesService.GetSelectedMarketingPreferences(marketingPreferencesViewModel, managerRoot, contact.ExmKeyBehaviorCache()?.MarketingPreferences).ToList();
                        _marketingPreferenceService.SavePreferences(contact, marketingPreferences);                        
                        return;
                    }
                }
            }

            var customXConnectContact = _xConnectContactFactory.CreateContactWithEmail(contactIdentifier.Identifier);
            _xConnectContactService.IdentifyCurrent(customXConnectContact);
            _xConnectContactService.UpdateOrCreateContact(customXConnectContact);

            var newContact = _xConnectContactService.GetXConnectContact(contactIdentifier, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);
            if (newContact == null)
            {
                throw new ContactException();
            }

            _saveMarketingPreferencesService.SetPersonalInformation(data, fields, newContact, contactIdentifier);
            _saveMarketingPreferencesService.SetExmKeyBehaviorCache(newContact);

            var contactList = _saveMarketingPreferencesService.GetAndValidateContactList(marketingPreferencesViewModel, _useDoubleOptIn);
            var newMarketingPreferences = _saveMarketingPreferencesService.GetSelectedMarketingPreferences(marketingPreferencesViewModel, managerRoot, newContact.ExmKeyBehaviorCache()?.MarketingPreferences).ToList();

            if (newMarketingPreferences.All(x => x.Preference == false))
            {
                UnsubscribeFromAll(newContact, contactIdentifier, managerRoot, contactList.ContactListDefinition.Id);
                return;
            }

            SubscribeContact(managerRoot, contactList, newContact, newMarketingPreferences);
        }

        private void UnsubscribeFromAll(Contact contact, ContactIdentifier contactIdentifier, ManagerRoot managerRoot, Guid listId)
        {
            _saveMarketingPreferencesService.ResetExmKeyBehaviorCache(contactIdentifier);
            _exmSubscriptionManager.UnsubscribeFromAll(contact, managerRoot);
        }

        private void SubscribeContact(ManagerRoot managerRoot, ContactList contactList, Contact contact, List<MarketingPreference> marketingPreferences)
        {
            if (_useDoubleOptIn)
            {
                if (contactList == null || contactList.ContactListDefinition.Id == Guid.Empty || contactList.ContactListDefinition.Type == ListType.SegmentedList)
                {
                    throw new ContactListException();
                }

                // If DoubleOptIn should be used the contact is subscribed to the selected contact list and gets a confirmation mail
                // The contact is only subscribed to the global contact list, which is used as source for the segmented lists, if he confirms the link in the mail
                _exmSubscriptionManager.Subscribe(contact, contactList.ContactListDefinition.Id, managerRoot, true);
            }

            _marketingPreferenceService.SavePreferences(contact, marketingPreferences);
        }

        protected abstract ContactIdentifier GetContactIdentifier(T data, IEnumerable<IViewModel> fields);
    }
}
