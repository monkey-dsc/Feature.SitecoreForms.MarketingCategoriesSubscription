using System;
using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.MarketingPreferences;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.MarketingPreferencesIdentifier.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Sitecore;
using Sitecore.Configuration;
using Sitecore.Data;
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
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.MarketingPreferencesIdentifier.Base
{
    public abstract class MarketingPreferencesIdentifierBase<T> : SubmitActionBase<T> where T : MarketingPreferencesIdentifierData
    {
        private readonly IXConnectService _xConnectService;
        private readonly IExmContactService _exmContactService;
        private readonly IExmSubscriptionManager _exmSubscriptionManager;
        private readonly ICustomMarketingPreferencesService _marketingPreferenceService;
        private readonly IClientApiService _clientApiService;
        private readonly IManagerRootService _managerRootService;
        private readonly ListManagerWrapper _listManagerWrapper;
        private readonly bool _useDoubleOptIn = Settings.GetBoolSetting("NewsletterSubscription.UseDoubleOptInForSubscription", true); // GDPR, sorry for the default value!

        protected new readonly ILogger Logger;
        private readonly IXConnectContactFactory _xConnectContactFactory;

        protected MarketingPreferencesIdentifierBase(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectContactFactory xConnectContactFactory,
            IXConnectService xConnectService,
            IExmContactService exmContactService,
            IExmSubscriptionManager exmSubscriptionManager,
            ICustomMarketingPreferencesService marketingPreferenceService,
            IClientApiService clientApiService,
            IManagerRootService managerRootService,
            ListManagerWrapper listManagerWrapper) : base(submitActionData)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(xConnectContactFactory, nameof(xConnectContactFactory)).IsNotNull();
            Condition.Requires(xConnectService, nameof(xConnectService)).IsNotNull();
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            Condition.Requires(exmSubscriptionManager, nameof(exmSubscriptionManager)).IsNotNull();
            Condition.Requires(marketingPreferenceService, nameof(marketingPreferenceService)).IsNotNull();
            Condition.Requires(clientApiService, nameof(clientApiService)).IsNotNull();
            Condition.Requires(managerRootService, nameof(managerRootService)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();

            Logger = logger;
            _xConnectContactFactory = xConnectContactFactory;
            _xConnectService = xConnectService;
            _exmContactService = exmContactService;
            _exmSubscriptionManager = exmSubscriptionManager;
            _marketingPreferenceService = marketingPreferenceService;
            _clientApiService = clientApiService; // ToDo: Maybe use this service if you want to send a unsubscribe from all notification!
            _managerRootService = managerRootService;
            _listManagerWrapper = listManagerWrapper;
        }

        protected override bool Execute(T data, FormSubmitContext formSubmitContext)
        {
            Assert.ArgumentNotNull(formSubmitContext, nameof(formSubmitContext));

            if (data.FieldEmailAddressId == Guid.Empty)
            {
                Logger.LogWarn("Field email address couldn't be empty!");
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
                Logger.LogWarn("A contact must have a valid identifiersource and identifiervalue!");
                return;
            }

            var customXConnectContact = _xConnectContactFactory.CreateContactWithEmail(contactIdentifier.Identifier);

            _xConnectService.UpdateOrCreateContact(customXConnectContact);

            var contact = _xConnectService.GetXConnectContact(contactIdentifier, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);
            if (contact == null)
            {
                Logger.LogWarn("Contact not found!");
                return;
            }

            _exmContactService.EnsureExmKeyBehaviorCache(contact);

            var newsletterSubscriptionViewModel = GetNewsletterSubscriptionViewModel(fields);
            if (newsletterSubscriptionViewModel == null)
            {
                Logger.LogError("Newsletter Subscription field is null!");
                return;
            }

            var managerRoot = GetManagerRoot(newsletterSubscriptionViewModel);
            if (managerRoot == null)
            {
                Logger.LogError("ManagerRoot is null! In order to subscribe a contact to marketing categories you have to select a valid manager root!");
                return;
            }

            var contactList = GetContactList(newsletterSubscriptionViewModel);
            if (contactList == null)
            {
                Logger.LogError("ContactList is null!");
            }

            var marketingPreferences = GetSelectedMarketingPreferences(newsletterSubscriptionViewModel, managerRoot, contact.ExmKeyBehaviorCache()?.MarketingPreferences).ToList();
            var unsubscribeFromAll = marketingPreferences.All(x => x.Preference == false);
            if (unsubscribeFromAll)
            {
                // ToDo: There is no service message sent, maybe implement it manually!
                _marketingPreferenceService.SavePreferences(contact, marketingPreferences);
                _exmSubscriptionManager.UnsubscribeFromAll(contact, managerRoot);
            }
            else
            {
                SubscribeContact(managerRoot, contactList, contact, marketingPreferences);
            }
        }

        private ManagerRoot GetManagerRoot(MarketingPreferencesViewModel model)
        {
            return model == null || string.IsNullOrEmpty(model.ManagerRootId) ? null : _managerRootService.GetManagerRoot(new Guid(model.ManagerRootId));
        }

        private ContactList GetContactList(MarketingPreferencesViewModel model)
        {
            return model == null || string.IsNullOrEmpty(model.ContactListId) ? null : _listManagerWrapper.FindById(new Guid(model.ContactListId));
        }

        private static IEnumerable<MarketingPreference> GetSelectedMarketingPreferences(MarketingPreferencesViewModel model, ManagerRoot managerRoot, IReadOnlyCollection<MarketingPreference> contactMarketingPreferences)
        {
            var preferencesList = new List<MarketingPreference>();
            if (model == null || !model.Value.Any())
            {
                return preferencesList;
            }

            foreach (var listFieldItem in model.Items)
            {
                if (contactMarketingPreferences != null && contactMarketingPreferences.Any(x => x.MarketingCategoryId.ToString("B").ToUpper() == listFieldItem.Value))
                {
                    preferencesList.Add(CreateMarketingPreference(managerRoot, listFieldItem.Value, listFieldItem.Selected));
                }
                else if (model.Value.Contains(listFieldItem.Value))
                {
                    preferencesList.Add(CreateMarketingPreference(managerRoot, listFieldItem.Value, true));
                }
            }

            return preferencesList;
        }

        private static MarketingPreference CreateMarketingPreference(ManagerRoot managerRoot, string preferenceId, bool selectedPreference)
        {
            var database = Context.ContentDatabase ?? Context.Database;
            var marketingPreference = new MarketingPreference
            {
                DateTime = DateTime.UtcNow,
                ManagerRootId = managerRoot.Id,
                MarketingCategoryId = database.GetItem(new ID(preferenceId)).ID.ToGuid(),
                Preference = selectedPreference
            };
            return marketingPreference;
        }

        private void SubscribeContact(ManagerRoot managerRoot, ContactList contactList, Contact contact, List<MarketingPreference> marketingPreferences)
        {
            if (_useDoubleOptIn)
            {
                if (managerRoot == null)
                {
                    throw new ArgumentNullException(nameof(managerRoot), "To subscribe a contact to a contact list, a valid manager root is needed!");
                }

                if (contactList.ContactListDefinition.Id == Guid.Empty || contactList.ContactListDefinition.Type == ListType.SegmentedList)
                {
                    throw new ArgumentNullException(nameof(contactList), "To subscribe a contact to a contact list, a valid contact list is needed and the list should not be segmented!");
                }

                // If DoubleOptIn should be used the contact is subscribed to the selected contact list and gets a confirmation mail
                // The contact is only subscribed to the global contact list which is used as source for the segmented lists if he confirms the mail
                // ToDo: Write a hint in your blog post to use a global contact list as source of the segmented lists!!!


                // ToDo: Check here whether the contact is still subscribed for other categories to determine if subscriptionConfirmation should be true or false
                // ToDo: Subscribe checks IsSubscribed before sending the confirmation mail, check this twice
                // TODO: ALSO CHECK IF CONTACT IS SUBSCRIBED TO OTHER CATEGORIES AND DON'T REMOVE HIM FROM THE GLOBAL LIST!
                // TODO: SUBSCRIPTION CONFIRMATION IS ALWAYS TRUE! SO EVERY TIME A CONFIRMATION MAIL WILL BE SEND... IS THAT EXPECTED?
                _exmSubscriptionManager.Subscribe(contact, contactList.ContactListDefinition.Id, managerRoot, true);
            }

            _marketingPreferenceService.SavePreferences(contact, marketingPreferences);
        }

        private static MarketingPreferencesViewModel GetNewsletterSubscriptionViewModel(IEnumerable<IViewModel> fields)
        {
            var newsletterSubscriptionField = fields.Where(field => field.GetType() == typeof(MarketingPreferencesViewModel)).Cast<MarketingPreferencesViewModel>().FirstOrDefault();
            return newsletterSubscriptionField;
        }

        protected abstract ContactIdentifier GetContactIdentifier(T data, IEnumerable<IViewModel> fields);
    }
}
