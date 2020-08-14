using System;
using System.Collections.Generic;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services
{
    public interface IMarketingPreferencesService
    {
        List<MarketingPreference> GetPreferences(Contact contact,  Guid managerRootId);
        List<MarketingPreference> SavePreferences(Contact contact,  List<MarketingPreference> preferences);
    }
}
