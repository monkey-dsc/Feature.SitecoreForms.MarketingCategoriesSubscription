using System;
using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Exceptions;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Data;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;
using Sitecore.ListManagement;
using Sitecore.Marketing.Definitions.ContactLists;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services
{
    public class SaveMarketingPreferencesService<T> : ISaveMarketingPreferencesService<T> where T : SaveMarketingPreferencesData
    {
        private readonly IXConnectContactService _xConnectContactService;
        private readonly IExmContactService _exmContactService;
        private readonly IManagerRootService _managerRootService;
        private readonly ListManagerWrapper _listManagerWrapper;

        public SaveMarketingPreferencesService() : this(
            ServiceLocator.ServiceProvider.GetService<IXConnectContactService>(),
            ServiceLocator.ServiceProvider.GetService<IExmContactService>(),
            ServiceLocator.ServiceProvider.GetService<IManagerRootService>(),
            ServiceLocator.ServiceProvider.GetService<ListManagerWrapper>())
        {
        }

        public SaveMarketingPreferencesService(
            IXConnectContactService xConnectContactService,
            IExmContactService exmContactService,
            IManagerRootService managerRootService,
            ListManagerWrapper listManagerWrapper)
        {
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            Condition.Requires(managerRootService, nameof(managerRootService)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();
            _xConnectContactService = xConnectContactService;
            _exmContactService = exmContactService;
            _managerRootService = managerRootService;
            _listManagerWrapper = listManagerWrapper;
        }

        public bool AuthenticateContact(Contact contact)
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null || Tracker.Current.Contact.IsNew)
            {
                return false;
            }

            var contactXdbTrackerIdentifier = contact.Identifiers.FirstOrDefault(x => x.Source == ContactIdentifiers.XdbTracker);
            var trackerXdbTrackerIdentifier = Tracker.Current.Contact.Identifiers.FirstOrDefault(x => x.Source == ContactIdentifiers.XdbTracker);

            return contactXdbTrackerIdentifier != null && trackerXdbTrackerIdentifier != null && contactXdbTrackerIdentifier.Identifier == trackerXdbTrackerIdentifier.Identifier;
        }

        public void SetPersonalInformation(T data, IList<IViewModel> fields, Entity contact, ContactIdentifier contactIdentifier)
        {
            string firstName;
            string lastName;

            var personalInformation = contact.GetFacet<PersonalInformation>();
            if (personalInformation != null)
            {
                firstName = personalInformation.FirstName;
                lastName = personalInformation.LastName;
            }
            else
            {
                firstName = GetFieldById(data.FieldFirstNameId, fields);
                lastName = GetFieldById(data.FieldLastNameId, fields);
            }

            var newPersonalInformation = new PersonalInformation { FirstName = firstName, LastName = lastName };

            _xConnectContactService.UpdateContactFacet(
                contactIdentifier,
                PersonalInformation.DefaultFacetKey,
                x =>
                {
                    x.FirstName = firstName;
                    x.LastName = lastName;
                    x.PreferredLanguage = Context.Language.ToString();
                },
                () => newPersonalInformation);
        }

        public void SetExmKeyBehaviorCache(Contact contact)
        {
            _exmContactService.EnsureExmKeyBehaviorCache(contact);
        }

        public void ResetExmKeyBehaviorCache(ContactIdentifier contactIdentifier)
        {
            _xConnectContactService.UpdateContactFacet<ExmKeyBehaviorCache>(contactIdentifier, ExmKeyBehaviorCache.DefaultFacetKey, x => x.MarketingPreferences = new List<MarketingPreference>());
        }

        public MarketingPreferencesViewModel GetMarketingPreferencesViewModel(IEnumerable<IViewModel> fields)
        {
            var marketingPreferencesField = fields.Where(field => field.GetType() == typeof(MarketingPreferencesViewModel)).Cast<MarketingPreferencesViewModel>().FirstOrDefault();
            return marketingPreferencesField;
        }

        public ManagerRoot GetManagerRoot(MarketingPreferencesViewModel model)
        {
            return model == null || string.IsNullOrEmpty(model.ManagerRootId) ? null : _managerRootService.GetManagerRoot(new Guid(model.ManagerRootId));
        }

        public ContactList GetAndValidateContactList(MarketingPreferencesViewModel model, bool useDoubleOptIn)
        {
            if (model == null || string.IsNullOrEmpty(model.ContactListId))
            {
                return null;
            }

            var contactList = _listManagerWrapper.FindById(new Guid(model.ContactListId));
            ValidateContactList(contactList, useDoubleOptIn);
            return contactList;
        }

        public IEnumerable<MarketingPreference> GetSelectedMarketingPreferences(ListViewModel model, ManagerRoot managerRoot, IReadOnlyCollection<MarketingPreference> contactMarketingPreferences)
        {
            var preferencesList = new List<MarketingPreference>();

            if (model == null || !model.Value.Any())
            {
                return preferencesList;
            }

            foreach (var listFieldItem in model.Items)
            {
                if (listFieldItem.Selected)
                {
                    preferencesList.Add(CreateMarketingPreference(managerRoot, listFieldItem.Value, true));
                }
                else if (contactMarketingPreferences != null && contactMarketingPreferences.Any(x => x.MarketingCategoryId.ToString("B").ToUpper() == listFieldItem.Value))
                {
                    preferencesList.Add(CreateMarketingPreference(managerRoot, listFieldItem.Value, listFieldItem.Selected));
                }
            }

            return preferencesList;
        }

        private static string GetFieldById(Guid? guid, IEnumerable<IViewModel> fields)
        {
            var fieldList = fields.ToList();
            var field = (StringInputViewModel)fieldList.FirstOrDefault(x => x.ItemId == guid.ToString());
            return field != null ? field.Value : string.Empty;
        }


        private static void ValidateContactList(ContactList contactList, bool useDoubleOptIn)
        {
            if (useDoubleOptIn && contactList == null || contactList.ContactListDefinition.Id == Guid.Empty || contactList.ContactListDefinition.Type == ListType.SegmentedList)
            {
                throw new ContactListException();
            }
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
    }
}
