using System.Collections.Generic;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.EmailCampaign.XConnect.Web;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact
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

        public Sitecore.XConnect.Contact GetKnownXConnectContactByEmailAddress()
        {
            var contactIdentifier = GetEmailAddressContactIdentifier();
            return contactIdentifier == null ? null : _contactService.GetContactWithRetry(contactIdentifier, Delay, RetryCount, ExmKeyBehaviorCache.DefaultFacetKey);
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

        private static ContactIdentifier GetEmailAddressContactIdentifier()
        {
            var current = Tracker.Current;
            var contact = current?.Contact;

            if (contact == null)
            {
                return null;
            }

            var xConnectFacets = Tracker.Current.Contact.GetFacet<IXConnectFacets>("XConnectFacets");
            if (xConnectFacets?.Facets == null)
            {
                return null;
            }

            if (!xConnectFacets.Facets.ContainsKey(EmailAddressList.DefaultFacetKey))
            {
                return null;
            }

            if (xConnectFacets.Facets[EmailAddressList.DefaultFacetKey] is EmailAddressList facet)
            {
                return GetValueFromEmailAddressListFacet(facet);
            }

            return null;
        }

        private static ContactIdentifier GetValueFromEmailAddressListFacet(EmailAddressList facet)
        {
            var preferredEmail = facet.PreferredEmail;
            var smtpAddress = preferredEmail?.SmtpAddress;

            return !string.IsNullOrEmpty(smtpAddress) ? new ContactIdentifier(ContactIdentifiers.Email, facet.PreferredEmail.SmtpAddress, ContactIdentifierType.Known) : null;
        }
    }
}
