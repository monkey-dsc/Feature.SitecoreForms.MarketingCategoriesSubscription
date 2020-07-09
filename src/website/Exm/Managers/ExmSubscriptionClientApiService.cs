using System;
using System.Runtime.CompilerServices;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Messaging;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    internal sealed class ExmSubscriptionClientApiService : IExmSubscriptionClientApiService
    {
        private readonly IMessageBus _subscribeContactMessagesBus;

        private readonly ILogger _logger;

        public ExmSubscriptionClientApiService(IMessageBus<SubscribeContactMessagesBus> subscribeContactMessagesBus, ILogger logger)
        {
            Condition.Requires<IMessageBus<SubscribeContactMessagesBus>>(subscribeContactMessagesBus, nameof(subscribeContactMessagesBus)).IsNotNull();
            Condition.Requires<ILogger>(logger, nameof(logger)).IsNotNull();
            _subscribeContactMessagesBus = subscribeContactMessagesBus;
            _logger = logger;
        }

        public void Subscribe(SubscribeContactMessage message)
        {
            Condition.Requires(message, nameof(message)).IsNotNull();
            _subscribeContactMessagesBus.Send(message, null);
            _logger.LogDebug(FormattableString.Invariant(FormattableStringFactory.Create("[BUS] Queued subscribe contact message. . ManagerRootId '{0}', RecipientListId '{1}', ContactIdentifier '{2}'.", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier)));
        }

        public void UnsubscribeFromAll(Contact contact, ManagerRoot managerRoot)
        {
            throw new NotImplementedException();
        }
    }
}
