using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services
{
    public interface IXConnectService
    {
        Contact GetXConnectContact(ContactIdentifier contactIdentifier, params string[] facetKeys);

        void UpdateOrCreateContact(IXConnectContactWithEmail contact);

        void UpdateContactFacet<T>(ContactIdentifier contactIdentifier, string facetKey, Action<T> updateFacets) where T : Facet, new();

        void UpdateContactFacet<T>(ContactIdentifier contactIdentifier, string facetKey, Action<T> updateFacets, Func<T> createFacet) where T : Facet;
    }
}
