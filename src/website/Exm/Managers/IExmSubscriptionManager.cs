using System;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    public interface IExmSubscriptionManager
    {
        bool Subscribe(Contact contact, Guid recipientListId, ManagerRoot managerRoot, bool subscriptionConfirmation);
        void UnsubscribeFromAll(Contact contact, ManagerRoot managerRoot);
    }
}
