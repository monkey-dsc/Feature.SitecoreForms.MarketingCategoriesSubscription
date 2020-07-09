using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Sitecore.EmailCampaign.Cm;
using Sitecore.EmailCampaign.Model.Messaging;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Framework.Messaging;
using Sitecore.Framework.Messaging.DeferStrategies;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging
{
    public class SubscribeContactMessageHandler : IMessageHandler<SubscribeContactMessage>, IMessageHandler
    {
        private readonly ILogger _logger;

        private readonly SubscriptionManager _subscriptionManager;

        private readonly IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> _deferStrategy;

        private readonly IMessageBus<SubscribeContactMessagesBus> _bus;
        private readonly IXConnectContactService _xConnectContactService;
        private readonly IManagerRootService _managerRootService;
        private readonly ListManagerWrapper _listManagerWrapper;

        public SubscribeContactMessageHandler(ILogger logger,
                                              ISubscriptionManager subscriptionManager,
                                              IDeferStrategy<DeferDetectionByResultBase<HandlerResult>> deferStrategy,
                                              IMessageBus<SubscribeContactMessagesBus> bus,
                                              IXConnectContactService xConnectContactService,
                                              IManagerRootService managerRootService,
                                              ListManagerWrapper listManagerWrapper)
        {
            Condition.Requires(logger, "logger").IsNotNull();
            Condition.Requires(subscriptionManager, nameof(subscriptionManager)).IsNotNull();
            Condition.Requires(subscriptionManager is SubscriptionManager).IsTrue("(Sub)-Type of SubscriptionManager required, because specific methods not in interface are used to send mails.");
            Condition.Requires(deferStrategy, nameof(deferStrategy)).IsNotNull();
            Condition.Requires(bus, nameof(bus)).IsNotNull();
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            Condition.Requires(managerRootService, nameof(managerRootService)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();
            _logger = logger;
            _subscriptionManager = (SubscriptionManager)subscriptionManager;
            _deferStrategy = deferStrategy;
            _bus = bus;
            _xConnectContactService = xConnectContactService;
            _managerRootService = managerRootService;
            _listManagerWrapper = listManagerWrapper;
        }

        public async Task Handle(SubscribeContactMessage message, IMessageReceiveContext receiveContext, IMessageReplyContext replyContext)
        {
            Condition.Requires<SubscribeContactMessage>(message, "message").IsNotNull<SubscribeContactMessage>();
            Condition.Requires<IMessageReceiveContext>(receiveContext, "receiveContext").IsNotNull<IMessageReceiveContext>();
            Condition.Requires<IMessageReplyContext>(replyContext, "replyContext").IsNotNull<IMessageReplyContext>();
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
        protected HandlerResult SubscribeContact(SubscribeContactMessage message)
        {
            _logger.LogDebug(FormattableString.Invariant(FormattableStringFactory.Create("[{0}] Subscribe contact to recipient list. ManagerRootId '{1}', RecipientListId '{2}', ContactIdentifier '{3}'.", new object[] { "SubscribeContactMessageHandler", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier })));

            var managerRoot = _managerRootService.GetManagerRoot(message.ManagerRootId);
            var contact = _xConnectContactService.GetXConnectContact(message.ContactIdentifier, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);

            var result =  HandleSubscriptionInternal(message.RecipientListId, contact, managerRoot, message.SendSubscriptionConfirmation);

            if (result)
            {
                return new HandlerResult(HandlerResultType.Successful);
            }
            _logger.LogError(FormattableString.Invariant(FormattableStringFactory.Create("[{0}] Failed to subscribe contact to recipient list. ManagerRootId '{1}', RecipientListId '{2}', ContactIdentifier '{3}'.", new object[] { "SubscribeContactMessageHandler", message.ManagerRootId, message.RecipientListId, message.ContactIdentifier?.Identifier })));
            return new HandlerResult(HandlerResultType.Error);
        }

        private bool HandleSubscriptionInternal(Guid recipientListId, Contact contact, ManagerRoot managerRoot, bool sendConfirmationMail)
        {
            if (_listManagerWrapper.FindById(recipientListId) == null)
            {
                return false;
            }

            var isSubscribed = _listManagerWrapper.IsSubscribed(recipientListId, contact);
            if (isSubscribed && !managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact))
            {
                return true;
            }

            if (sendConfirmationMail)
            {
                return _subscriptionManager.SendConfirmationMessage(contact, recipientListId, managerRoot);
            }

            var wasSuccessful = true;
            if (!isSubscribed)
            {
                wasSuccessful = _listManagerWrapper.SubscribeContact(recipientListId, contact);
            }

            if (!_subscriptionManager.SendSubscriptionNotification(managerRoot, contact))
            {
                var alias = contact.GetAlias();
                _logger.LogError($"Failed to send subscription notification to {alias?.ToLogFile()}");
            }

            if (managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact) && !_subscriptionManager.RemoveContactFromList(contact, managerRoot.GlobalSubscription.GetGlobalOptOutListId()))
            {
                var alias = contact.GetAlias();
                _logger.LogError($"Failed to remove {alias?.ToLogFile()} from global opt out list");
            }

            return wasSuccessful;
        }
    }
}
