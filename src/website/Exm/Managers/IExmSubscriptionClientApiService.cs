using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    public interface IExmSubscriptionClientApiService
    {
        void Subscribe(SubscribeContactMessage message);
        void UnsubscribeFromAll(Contact contact, ManagerRoot managerRoot);
    }
}
