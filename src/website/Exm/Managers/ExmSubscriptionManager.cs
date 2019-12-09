using System;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Diagnostics;
using Sitecore.EmailCampaign.Cm;
using Sitecore.EmailCampaign.Cm.Factories;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Marketing.Core.Extensions;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.Factories;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Schema;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers
{
    internal sealed class ExmSubscriptionManager : SubscriptionManager, IExmSubscriptionManager
    {
        private readonly ILogger _logger;
        private readonly ListManagerWrapper _listManagerWrapper;

        public ExmSubscriptionManager() : this(
            ServiceLocator.ServiceProvider.GetService<IContactService>(),
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<ListManagerWrapper>(),
            ServiceLocator.ServiceProvider.GetService<IExmCampaignService>(),
            ServiceLocator.ServiceProvider.GetService<PipelineHelper>(),
            ServiceLocator.ServiceProvider.GetService<ISendingManagerFactory>(),
            ServiceLocator.ServiceProvider.GetService<IManagerRootService>(),
            ServiceLocator.ServiceProvider.GetService<IRecipientManagerFactory>())
        {
            
        }

        public ExmSubscriptionManager(
            IContactService contactService,
            ILogger logger,
            ListManagerWrapper listManagerWrapper,
            IExmCampaignService exmCampaignService,
            PipelineHelper pipelineHelper,
            ISendingManagerFactory sendingManagerFactory,
            IManagerRootService managerRootService,
            IRecipientManagerFactory recipientManagerFactory) : base(contactService, logger, listManagerWrapper, exmCampaignService, pipelineHelper, sendingManagerFactory, managerRootService, recipientManagerFactory)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();
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

            var flag1 = _listManagerWrapper.IsSubscribed(recipientListId, contact);
            if (flag1 && !managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact))
            {
                return true;
            }

            if (subscriptionConfirmation)
            {
                return SendConfirmationMessage(contact, recipientListId, managerRoot);
            }

            var flag2 = true;
            if (!flag1)
            {
                flag2 = _listManagerWrapper.SubscribeContact(recipientListId, contact);
            }

            if (!SendSubscriptionNotification(managerRoot, contact))
            {
                var alias = contact.GetAlias();
                var message = $"Failed to send subscription notification to {alias?.ToLogFile()}";
                _logger.LogError(message);
            }

            if (managerRoot.GlobalSubscription.IsInDefaultExcludeCollection(contact) && !RemoveContactFromList(contact, managerRoot.GlobalSubscription.GetGlobalOptOutListId()))
            {
                var alias = contact.GetAlias();
                var message = $"Failed to remove {alias?.ToLogFile()} from global opt out list";
                _logger.LogError(message);
            }

            return flag2;
        }
    }
}
