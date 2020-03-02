using System.Collections.Generic;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.ListManagement;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services
{
    public interface ISaveMarketingPreferencesService<in T> where T : SaveMarketingPreferencesData
    {
        bool AuthenticateContact(Contact contact);
        void SetPersonalInformation(T data, IList<IViewModel> fields, Entity contact, ContactIdentifier contactIdentifier);
        void SetExmKeyBehaviorCache(Contact contact);
        void ResetExmKeyBehaviorCache(ContactIdentifier contactIdentifier);
        MarketingPreferencesViewModel GetMarketingPreferencesViewModel(IEnumerable<IViewModel> fields);
        ManagerRoot GetManagerRoot(MarketingPreferencesViewModel model);
        ContactList GetAndValidateContactList(MarketingPreferencesViewModel model, bool useDoubleOptIn);
        IEnumerable<MarketingPreference> GetSelectedMarketingPreferences(ListViewModel model, ManagerRoot managerRoot, IReadOnlyCollection<MarketingPreference> contactMarketingPreferences);
    }
}
