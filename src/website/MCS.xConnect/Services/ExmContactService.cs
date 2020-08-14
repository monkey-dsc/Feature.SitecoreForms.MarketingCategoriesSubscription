using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.EmailCampaign.XConnect.Web;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.XConnect.Client;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services
{
    internal sealed class ExmContactService : ContactService, IExmContactService
    {
        private readonly IContactService _contactService;
        private readonly XConnectRetry _xConnectRetry;

        private const double Delay = 100;
        private const int RetryCount = 3;

        public ExmContactService() : this(
            ServiceLocator.ServiceProvider.GetService<IContactService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectClientFactory>(),
            ServiceLocator.ServiceProvider.GetService<XConnectRetry>(),
            ServiceLocator.ServiceProvider.GetService<ILogger>())
        {
        }

        public ExmContactService(
            IContactService contactService,
            IXConnectClientFactory xConnectClientFactory,
            XConnectRetry xConnectRetry,
            ILogger logger) : base(xConnectClientFactory, xConnectRetry, logger)
        {
            Condition.Requires(contactService, nameof(contactService)).IsNotNull();
            Condition.Requires(xConnectRetry, nameof(xConnectRetry)).IsNotNull();
            _contactService = contactService;
            _xConnectRetry = xConnectRetry;
        }

        public void EnsureExmKeyBehaviorCache(Sitecore.XConnect.Contact contact)
        {
            var facet = contact.ExmKeyBehaviorCache();
            if (facet == null)
            {
                facet = new ExmKeyBehaviorCache
                {
                    MarketingPreferences = new List<MarketingPreference>()
                };
            }
            else
            {
                facet.MarketingPreferences = facet.MarketingPreferences ?? new List<MarketingPreference>();
            }

            _xConnectRetry.RequestWithRetry(
                client =>
                {
                    client.SetExmKeyBehaviorCache(contact, facet);
                    client.SubmitAsync();
                },
                Delay,
                RetryCount);
        }

        public void ResetExmKeyBehaviorCache(Sitecore.XConnect.Contact contact)
        {
            var facet = new ExmKeyBehaviorCache
            {
                MarketingPreferences = new List<MarketingPreference>()
            };

            _xConnectRetry.RequestWithRetry(
                client =>
                {
                    client.SetExmKeyBehaviorCache(contact, facet);
                    client.Submit();
                },
                Delay,
                RetryCount);
        }
    }
}
