using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories
{
    public interface IXConnectContactRepository
    {
        void UpdateOrCreateXConnectContactWithEmail(IXConnectContactWithEmail xConnectContact);

        void UpdateContactFacet<T>(
            IdentifiedContactReference reference,
            ContactExpandOptions expandOptions,
            Action<T> updateFacets,
            Func<T> createFacet)
            where T : Facet;
    }
}
