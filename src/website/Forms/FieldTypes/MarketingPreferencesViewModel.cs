using System;
using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.MarketingPreferences;
using Microsoft.Extensions.DependencyInjection;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Mvc.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.SecurityModel;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes
{
    [Serializable]
    public class MarketingPreferencesViewModel : CheckBoxListViewModel
    {
        private readonly IExmContactService _exmContactService;
        private readonly IManagerRootService _managerRootService;
        private readonly ICustomMarketingPreferencesService _marketingPreferencesService;
        private readonly ILogger _logger;

        public MarketingPreferencesViewModel() : this(
            ServiceLocator.ServiceProvider.GetService<IExmContactService>(),
            ServiceLocator.ServiceProvider.GetService<IManagerRootService>(),
            ServiceLocator.ServiceProvider.GetService<ICustomMarketingPreferencesService>(),
            ServiceLocator.ServiceProvider.GetService<ILogger>())
        {
        }

        public MarketingPreferencesViewModel(
            IExmContactService exmContactService,
            IManagerRootService managerRootService,
            ICustomMarketingPreferencesService marketingPreferencesService,
            ILogger logger)
        {
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            Condition.Requires(managerRootService, nameof(managerRootService)).IsNotNull();
            Condition.Requires(marketingPreferencesService, nameof(marketingPreferencesService)).IsNotNull();
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            _exmContactService = exmContactService;
            _managerRootService = managerRootService;
            _marketingPreferencesService = marketingPreferencesService;
            _logger = logger;
        }

        // Todo: Currently the TreeList is also displaying "Site"-Templates of Ulm, this should be removed if it goes public
        // ToDo: Write a hint in your blog that this must be adjusted if Manager Roots are nested!!
        public string ContactListId { get; set; }
        public string ManagerRootId { get; set; }

        protected override void InitItemProperties(Item item)
        {
            base.InitItemProperties(item);
            ContactListId = StringUtil.GetString(item.Fields["Contact List Id"]);
            ManagerRootId = StringUtil.GetString(item.Fields["Manager Root Id"]);
            RenderListItems(item, ManagerRootId);
        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            item.Fields["Contact List Id"]?.SetValue(ContactListId, true);
            item.Fields["Manager Root Id"]?.SetValue(ManagerRootId, true);
            RenderListItems(item, ManagerRootId);
        }

        private void RenderListItems(Item item, string selectedManagerRootId)
        {
            // We have to activate the Tracker because it's only activated on a form submit
            // See: https://sitecore.stackexchange.com/questions/11673/analytics-tracker-current-is-null-in-sitecore-9-update-1-forms-submit-action-wit
            // Note: The mentioned behavior is expected because the tracker should be only triggered upon a submit as it is expensive to keep it alive after navigating to the next or previous page
            // But, we want to identify the contact and set the preferences of the contact as already checked
            StartTracker();

            var database = Context.ContentDatabase ?? Context.Database;
            var managerRoot = _managerRootService.GetManagerRoot(new Guid(selectedManagerRootId));

            if (managerRoot == null)
            {
                _logger.LogError("You have to select a valid Manager Root!");
                return;
            }

            // ToDo: Comment this if you don't want to identify the contact and check the marketing preferences of him
            var knownContact = _exmContactService.GetKnownXConnectContactByEmailAddress();
            var marketingPreferences = new List<MarketingPreference>();
            if (knownContact != null)
            {
                marketingPreferences = _marketingPreferencesService.GetPreferences(knownContact, managerRoot.Id);
            }

            var marketingCategoryGroups = managerRoot.Settings.MarketingCategoryGroups.Select(database.GetItem).ToList();
            if (!marketingCategoryGroups.Any())
            {
                _logger.LogWarn("no marketing groups are associated to the manager root!");
                return;
            }

            foreach (var marketingCategoryGroup in marketingCategoryGroups)
            {
                var marketingCategories = marketingCategoryGroup.Children;
                foreach (Item marketingCategory in marketingCategories)
                {
                    var categoryListItem = new ListFieldItem();
                    categoryListItem.ItemId = categoryListItem.Value = marketingCategory.ID.ToString();
                    categoryListItem.Text = marketingCategory.DisplayName;
                    categoryListItem.Selected = IsSelected(marketingPreferences, marketingCategory);
                    Items.Add(categoryListItem);
                }
            }

            using (new SecurityDisabler())
            {
                base.UpdateDataSourceSettings(item);
            }
        }

        private static bool IsSelected(IEnumerable<MarketingPreference> contactMarketingPreferences, Item marketingCategory)
        {
            return contactMarketingPreferences != null && contactMarketingPreferences.Any(contactMarketingPreference => contactMarketingPreference.MarketingCategoryId == marketingCategory.ID.Guid && contactMarketingPreference.Preference == true);
        }

        private static void StartTracker()
        {
            if (Tracker.Current == null && Tracker.Enabled)
            {
                Tracker.StartTracking();
            }
        }
    }
}
