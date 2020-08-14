using System;
using Sitecore.XConnect;
using Sitecore.Globalization;
using Sitecore.Modules.EmailCampaign;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging
{
    public interface ISubscribeContactService
    {
        bool HandleContactSubscription(Guid messageRecipientListId, ContactIdentifier messageContactIdentifier, Guid messageManagerRootId, Language messageContextLanguage, bool messageSendSubscriptionConfirmation);
        bool HandleContactUnsubscriptionFromAll(ContactIdentifier contact, ManagerRoot managerRoot);
    }
}
