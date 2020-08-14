using System;
using System.Collections.Generic;
using Sitecore.EmailCampaign.Cd.Services;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.Framework.Conditions;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CD.Services
{
    internal sealed class MarketingPreferencesService : Contract.Services.IMarketingPreferencesService
    {
        private readonly IMarketingPreferencesService _marketingPreferencesService;

        public MarketingPreferencesService(IMarketingPreferencesService marketingPreferencesService)
        {
            Condition.Requires(marketingPreferencesService, nameof(marketingPreferencesService)).IsNotNull();

            _marketingPreferencesService = marketingPreferencesService;
        }

        public List<MarketingPreference> GetPreferences(Contact contact, Guid managerRootId)
        {
            return _marketingPreferencesService.GetPreferences(contact, managerRootId);
        }

        public List<MarketingPreference> SavePreferences(Contact contact, List<MarketingPreference> preferences)
        {
            return _marketingPreferencesService.SavePreferences(contact, preferences);
        }
    }
}
