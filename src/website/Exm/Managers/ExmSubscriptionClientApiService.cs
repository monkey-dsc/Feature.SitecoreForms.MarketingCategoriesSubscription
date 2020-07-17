using System;
using System.Runtime.CompilerServices;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging;
using Sitecore.EmailCampaign.Cd.Services;
using Sitecore.EmailCampaign.Model.Messaging;
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
        private readonly IClientApiService _clientApiService;

        public ExmSubscriptionClientApiService(IMessageBus<SubscribeContactMessagesBus> subscribeContactMessagesBus, ILogger logger, IClientApiService clientApiService)
        {
            Condition.Requires(subscribeContactMessagesBus, nameof(subscribeContactMessagesBus)).IsNotNull();
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(clientApiService, nameof(clientApiService)).IsNotNull();
            _subscribeContactMessagesBus = subscribeContactMessagesBus;
            _logger = logger;
            _clientApiService = clientApiService;
        }

        public void Subscribe(SubscribeContactMessage message)
        {
            Condition.Requires(message, nameof(message)).IsNotNull();
            _subscribeContactMessagesBus.Send(message, null);
            _logger.LogDebug(FormattableString.Invariant(FormattableStringFactory.Create("[BUS] Queued subscribe contact message. . ManagerRootId '{0}', RecipientListId '{1}', ContactIdentifier '{2}'.", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier)));
        }

        public void UnsubscribeFromAll(ContactIdentifier contactIdentifier, ManagerRoot managerRoot)
        {
            _clientApiService.UpdateListSubscription(new UpdateListSubscriptionMessage()
            {
                ListSubscribeOperation = ListSubscribeOperation.UnsubscribeFromAll,
                ContactIdentifier = contactIdentifier,
                MessageId = Guid.Empty, //note: not used for unsubscribeFromAll, so it can be any guid
                ManagerRootId = managerRoot.Id
            });
        }
    }
}
