using Sitecore.Modules.EmailCampaign.Core.Contacts;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services
{
    public interface IExmContactService : IContactService
    {
        void EnsureExmKeyBehaviorCache(Sitecore.XConnect.Contact contact);
        void ResetExmKeyBehaviorCache(Sitecore.XConnect.Contact contact);
    }
}
