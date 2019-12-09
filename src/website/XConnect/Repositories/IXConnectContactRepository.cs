using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories
{
    public interface IXConnectContactRepository
    {
        // ToDo: Review this, currently not in use!
        void ReloadContactDataIntoSession();
        
        // ToDo: Review this, currently not in use!
        void SaveNewContactToCollectionDb(Sitecore.Analytics.Tracking.Contact contact);

        void UpdateOrCreateXConnectContactWithEmail(IXConnectContactWithEmail xConnectContact);
        
        // ToDo: Review this, currently not in use!
        void UpdateContactFacet<T>(
            IdentifiedContactReference reference,
            ContactExpandOptions expandOptions,
            Action<T> updateFacets,
            Func<T> createFacet)
            where T : Facet;
    }
}
