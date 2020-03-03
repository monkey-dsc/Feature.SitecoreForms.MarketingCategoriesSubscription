using System;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Cm;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Marketing.Core.Extensions;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.XConnect;
using Sitecore.XConnect.Schema;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    internal sealed class ExmSubscriptionManager : IExmSubscriptionManager
    {
        private readonly ILogger _logger;
        private readonly ListManagerWrapper _listManagerWrapper;
        private readonly SubscriptionManager _subscriptionManager;

        public ExmSubscriptionManager() : this(
            ServiceLocator.ServiceProvider.GetService<ISubscriptionManager>(),
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<ListManagerWrapper>())
        {
            
        }

        public ExmSubscriptionManager(
            ISubscriptionManager subscriptionManager,
            ILogger logger,
            ListManagerWrapper listManagerWrapper)
        {
            Condition.Requires(subscriptionManager, nameof(subscriptionManager)).IsNotNull();
            Condition.Requires(subscriptionManager is SubscriptionManager);
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();

            _subscriptionManager = (SubscriptionManager)subscriptionManager;
            _logger = logger;
            _listManagerWrapper = listManagerWrapper;
        }

        // Original code for this method was taken from Sitecore.EmailCampaign.Cm.SubscriptionManager,
        // this method was previously private but does exactly what we need.
        // We do not want to subscribe via a message, because contact, recipientListId, managerRoot are known
        public bool Subscribe(Contact contact, Guid recipientListId, ManagerRoot managerRoot, bool subscriptionConfirmation)
        {
            Assert.ArgumentNotNull(contact, nameof(contact));
            Assert.ArgumentNotNull(contact.Id, "Id");
            Condition.Requires(recipientListId, nameof(recipientListId)).IsNotEmptyGuid();
            Assert.ArgumentNotNull(managerRoot, nameof(managerRoot));
            var byId = _listManagerWrapper.FindById(recipientListId);
            if (byId == null)
            {
                return false;
            }

            var isSubscribed = _listManagerWrapper.IsSubscribed(recipientListId, contact);
            if (isSubscribed && !managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact))
            {
                return true;
            }

            if (subscriptionConfirmation)
            {
                return _subscriptionManager.SendConfirmationMessage(contact, recipientListId, managerRoot);
            }

            var flag2 = true;
            if (!isSubscribed)
            {
                flag2 = _listManagerWrapper.SubscribeContact(recipientListId, contact);
            }

            if (!_subscriptionManager.SendSubscriptionNotification(managerRoot, contact))
            {
                var alias = contact.GetAlias();
                var message = $"Failed to send subscription notification to {alias?.ToLogFile()}";
                _logger.LogError(message);
            }

            if (managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact) && !_subscriptionManager.RemoveContactFromList(contact, managerRoot.GlobalSubscription.GetGlobalOptOutListId()))
            {
                var alias = contact.GetAlias();
                var message = $"Failed to remove {alias?.ToLogFile()} from global opt out list";
                _logger.LogError(message);
            }

            return flag2;
        }

        public void UnsubscribeFromAll(Contact contact, ManagerRoot managerRoot)
        {
            _subscriptionManager.UnsubscribeFromAll(contact, managerRoot);
        }
    }
}
