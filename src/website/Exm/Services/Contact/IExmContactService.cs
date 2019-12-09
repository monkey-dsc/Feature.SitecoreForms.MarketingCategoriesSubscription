using Sitecore.Modules.EmailCampaign.Core.Contacts;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact
{
    public interface IExmContactService : IContactService
    {
        void EnsureExmKeyBehaviorCache(Sitecore.XConnect.Contact contact);
        void ResetExmKeyBehaviorCache(Sitecore.XConnect.Contact contact);
    }
}
