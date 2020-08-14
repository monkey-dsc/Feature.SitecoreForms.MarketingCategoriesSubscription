using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services;
using Sitecore.EmailCampaign.Cm;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Globalization;
using Sitecore.Marketing.Definitions.ContactLists;
using Sitecore.Modules.EmailCampaign;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging
{
    internal class SubscribeContactService : ISubscribeContactService
    {
        private readonly ILogger _logger;
        private readonly SubscriptionManager _subscriptionManager;
        private readonly IXConnectContactService _xConnectContactService;
        private readonly IManagerRootService _managerRootService;
        private readonly ListManagerWrapper _listManagerWrapper;

        public SubscribeContactService(ILogger logger,
                                       ISubscriptionManager subscriptionManager,
                                       IXConnectContactService xConnectContactService,
                                       IManagerRootService managerRootService,
                                       ListManagerWrapper listManagerWrapper)
        {
            Condition.Requires(logger, nameof(logger)).IsNotNull();
            Condition.Requires(subscriptionManager, nameof(subscriptionManager)).IsNotNull();
            Condition.Requires(subscriptionManager is SubscriptionManager).IsTrue("(Sub)-Type of SubscriptionManager required, because specific methods not in interface are used to send mails.");
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            Condition.Requires(managerRootService, nameof(managerRootService)).IsNotNull();
            Condition.Requires(listManagerWrapper, nameof(listManagerWrapper)).IsNotNull();

            _logger = logger;
            _subscriptionManager = (SubscriptionManager)subscriptionManager;
            _xConnectContactService = xConnectContactService;
            _managerRootService = managerRootService;
            _listManagerWrapper = listManagerWrapper;
        }


        public bool HandleContactSubscription(Guid messageRecipientListId, ContactIdentifier messageContactIdentifier, Guid messageManagerRootId, Language messageContextLanguage, bool messageSendSubscriptionConfirmation)
        {
            if (messageContactIdentifier == null)
            {
                throw new ArgumentNullException(nameof(messageContactIdentifier));
            }

            using (new LanguageSwitcher(messageContextLanguage))
            {
                var managerRoot = _managerRootService.GetManagerRoot(messageManagerRootId);
                var contact = _xConnectContactService.GetXConnectContact(messageContactIdentifier, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);

                return HandleContactSubscriptionInternal(messageRecipientListId, contact, managerRoot, messageSendSubscriptionConfirmation);
            }
        }

        public bool HandleContactUnsubscriptionFromAll(ContactIdentifier contactIdentifer, ManagerRoot managerRoot)
        {
            if (contactIdentifer == null)
            {
                throw new ArgumentNullException(nameof(contactIdentifer));
            }

            if (managerRoot == null)
            {
                throw new ArgumentNullException(nameof(managerRoot));
            }

            var contact = _xConnectContactService.GetXConnectContact(contactIdentifer, PersonalInformation.DefaultFacetKey, ExmKeyBehaviorCache.DefaultFacetKey, EmailAddressList.DefaultFacetKey, ListSubscriptions.DefaultFacetKey);
            return _subscriptionManager.UnsubscribeFromAll(contact, managerRoot);
        }


        private bool HandleContactSubscriptionInternal(Guid recipientListId, Contact contact, ManagerRoot managerRoot, bool sendConfirmationMail)
        {
            var contactList = _listManagerWrapper.FindById(recipientListId);
            if (contactList == null)
            {
                return false;
            }

            if (contactList.ContactListDefinition.Type == ListType.SegmentedList)
            {
                return false; //note: segmented list is not allowed to subscribe to!
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
