using System;
using Sitecore.EmailCampaign.Cm;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    public interface IExmSubscriptionManager : ISubscriptionManager
    {
        bool Subscribe(Contact contact, Guid recipientListId, ManagerRoot managerRoot, bool subscriptionConfirmation);
    }
}
