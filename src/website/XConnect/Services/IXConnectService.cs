using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services
{
    public interface IXConnectService
    {
        void CheckIdentifier(IXConnectContact contact);

        ContactIdentifier GetEmailContactIdentifierOfCurrentContact();

        Contact GetXConnectContact(ContactIdentifier contactIdentifier, params string[] facetKeys);

        Contact GetXConnectContactByEmailAddress();

        void IdentifyCurrent(IXConnectContact contact);

        void UpdateOrCreateContact(IXConnectContactWithEmail contact);

        void UpdateContactFacet<T>(ContactIdentifier contactIdentifier, string facetKey, Action<T> updateFacets) where T : Facet, new();

        void UpdateContactFacet<T>(ContactIdentifier contactIdentifier, string facetKey, Action<T> updateFacets, Func<T> createFacet) where T : Facet;
    }
}
