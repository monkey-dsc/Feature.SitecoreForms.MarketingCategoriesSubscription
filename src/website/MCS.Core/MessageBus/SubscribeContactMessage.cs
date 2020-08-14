using System;
using Sitecore.Globalization;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus
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

        public Language ContextLanguage
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
