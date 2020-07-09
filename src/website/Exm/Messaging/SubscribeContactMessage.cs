using System;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging
{
    public class SubscribeContactMessage
    {
        public ContactIdentifier ContactIdentifier
        {
            get;
            set;
        }

        public Guid RecipientListId
        {
            get;
            set;
        }

        public Guid ManagerRootId
        {
            get;
            set;
        }

        public bool SendSubscriptionConfirmation
        {
            get;
            set;
        }

        public SubscribeContactMessage()
        {
        }
    }
}
