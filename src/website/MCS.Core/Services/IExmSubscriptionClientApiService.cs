using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services
{
    public interface IExmSubscriptionClientApiService
    {
        void Subscribe(SubscribeContactMessage message);
        void UnsubscribeFromAll(ContactIdentifier contact, ManagerRoot managerRoot);
    }
}
