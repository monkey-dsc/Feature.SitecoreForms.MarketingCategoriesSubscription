using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Services
{
    internal sealed class ExmStandaloneSubscriptionClientApiService : IExmSubscriptionClientApiService
    {
        private readonly ISubscribeContactService _subscribeContactService;

        public ExmStandaloneSubscriptionClientApiService(ISubscribeContactService subscribeContactService)
        {
            Condition.Requires(subscribeContactService, nameof(subscribeContactService)).IsNotNull();

            _subscribeContactService = subscribeContactService;
        }

        public void Subscribe(SubscribeContactMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            _subscribeContactService.HandleContactSubscription(message.RecipientListId, message.ContactIdentifier, message.ManagerRootId, message.ContextLanguage, message.SendSubscriptionConfirmation);
        }

        public void UnsubscribeFromAll(ContactIdentifier contact, ManagerRoot managerRoot)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (managerRoot == null)
            {
                throw new ArgumentNullException(nameof(managerRoot));
            }

            _subscribeContactService.HandleContactUnsubscriptionFromAll(contact, managerRoot);
        }
    }
}
