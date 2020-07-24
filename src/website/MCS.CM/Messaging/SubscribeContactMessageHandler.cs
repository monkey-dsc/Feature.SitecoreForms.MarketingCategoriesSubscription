using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus;
using Sitecore.EmailCampaign.Model.Messaging;
using Sitecore.Framework.Messaging;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Messaging.DeferStrategies;
using Sitecore.Framework.Conditions;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging
{
    // ReSharper disable once UnusedMember.Global
    // note: instantiated by sitecore config
    public class SubscribeContactMessageHandler : IMessageHandler<SubscribeContactMessage>
    {
        private readonly ILogger _logger;
        private readonly IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> _deferStrategy;

        private readonly IMessageBus<SubscribeContactMessagesBus> _bus;
        private readonly ISubscribeContactService _subscribeContactService;

        public SubscribeContactMessageHandler(ILogger logger,
                                              IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> deferStrategy,
                                              IMessageBus<SubscribeContactMessagesBus> bus,
                                              ISubscribeContactService subscribeContactService)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(deferStrategy, nameof(deferStrategy)).IsNotNull();
            Condition.Requires(bus, nameof(bus)).IsNotNull();
            Condition.Requires(_subscribeContactService, nameof(_subscribeContactService)).IsNotNull();

            _logger = logger;
            _deferStrategy = deferStrategy;
            _bus = bus;
            _subscribeContactService = subscribeContactService;
        }

        public async Task Handle(SubscribeContactMessage message, IMessageReceiveContext receiveContext, IMessageReplyContext replyContext)
        {
            Condition.Requires(message, "message").IsNotNull();
            Condition.Requires(receiveContext, "receiveContext").IsNotNull();
            Condition.Requires(replyContext, "replyContext").IsNotNull();
            var configuredTaskAwaitable = _deferStrategy.ExecuteAsync(_bus, message, receiveContext, () => SubscribeContact(message)).ConfigureAwait(false);
            if (!(await configuredTaskAwaitable).Deferred)
            {
                _logger.LogDebug("[SubscribeContactMessageHandler] processed message.'");
            }
            else
            {
                _logger.LogDebug("[SubscribeContactMessageHandler] defered message.");
            }
        }

        private HandlerResult SubscribeContact(SubscribeContactMessage message)
        {
            _logger.LogDebug(FormattableString.Invariant(FormattableStringFactory.Create("[{0}] Subscribe contact to recipient list. ManagerRootId '{1}', RecipientListId '{2}', ContactIdentifier '{3}'.", new object[] { "SubscribeContactMessageHandler", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier })));

            var result = _subscribeContactService.HandleContactSubscription(message.RecipientListId, message.ContactIdentifier, message.ManagerRootId, message.ContextLanguage, message.SendSubscriptionConfirmation);

            if (result)
            {
                return new HandlerResult(HandlerResultType.Successful);
            }

            _logger.LogError(FormattableString.Invariant(FormattableStringFactory.Create("[{0}] Failed to subscribe contact to recipient list. ManagerRootId '{1}', RecipientListId '{2}', ContactIdentifier '{3}'.", new object[] { "SubscribeContactMessageHandler", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier })));
            return new HandlerResult(HandlerResultType.Error);
        }
    }
}
