using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Extensions;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Models;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Analytics.Tracking.Identification;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.Framework.Conditions;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services
{
    internal sealed class XConnectContactService : IXConnectContactService
    {
        private readonly IExmContactService _exmContactService;
        private readonly IXConnectContactRepository _xConnectContactRepository;
        private readonly IContactIdentificationManager _contactIdentificationManager;

        private const double Delay = 100;
        private const int RetryCount = 3;

        // ReSharper disable once UnusedMember.Global
        public XConnectContactService() : this(
            ServiceLocator.ServiceProvider.GetService<IExmContactService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactRepository>(),
            ServiceLocator.ServiceProvider.GetService<IContactIdentificationManager>())
        {
        }

        public XConnectContactService(IExmContactService exmContactService, IXConnectContactRepository xConnectContactRepository, IContactIdentificationManager contactIdentificationManager)
        {
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            Condition.Requires(xConnectContactRepository, nameof(xConnectContactRepository)).IsNotNull();
            Condition.Requires(contactIdentificationManager, nameof(contactIdentificationManager)).IsNotNull();
            _exmContactService = exmContactService;
            _xConnectContactRepository = xConnectContactRepository;
            _contactIdentificationManager = contactIdentificationManager;
        }

        public void CheckIdentifier(IXConnectContact contact)
        {
            if (contact == null)
            {
                throw new ArgumentNullException(nameof(contact));
            }

            if (string.IsNullOrEmpty(contact.IdentifierSource) || string.IsNullOrEmpty(contact.IdentifierValue))
            {
                throw new Exception("A contact must have an identifiersource and identifiervalue!");
            }
        }

        public ContactIdentifier GetEmailContactIdentifierOfCurrentContact()
        {
            var current = Tracker.Current;
            var contact = current?.Contact;

            if (contact == null)
            {
                return null;
            }

            var emailIdentifier = contact.Identifiers.AnalyticsContactIdentifierToXConnectContactIdentifierBySource(ContactIdentifiers.Email);
            if (emailIdentifier != null)
            {
                return emailIdentifier;
            }

            var xConnectFacets = Tracker.Current.Contact.GetFacet<IXConnectFacets>(FacetKeys.XConnectFacets);
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

        public Contact GetXConnectContact(ContactIdentifier contactIdentifier, params string[] facetKeys)
        {
            var contactWithRetry = _exmContactService.GetContactWithRetry(contactIdentifier, Delay, RetryCount, facetKeys);
            return contactWithRetry;
        }

        public Contact GetXConnectContactByEmailAddress()
        {
            var contactIdentifier = GetEmailContactIdentifierOfCurrentContact();
            return contactIdentifier == null ? null : _exmContactService.GetContactWithRetry(contactIdentifier, Delay, RetryCount, ExmKeyBehaviorCache.DefaultFacetKey);
        }

        public void IdentifyCurrent(IXConnectContact contact)
        {
            CheckIdentifier(contact);

            if (Tracker.Current == null || Tracker.Current.Contact == null || Tracker.Current.Contact.IsNew)
            {
                return;
            }

            if (Tracker.Current.Session == null)
            {
                return;
            }

            // ToDo: Remove old implementation
            // Tracker.Current.Session.IdentifyAs(contact.IdentifierSource, contact.IdentifierValue);
            var result = _contactIdentificationManager.IdentifyAs(new KnownContactIdentifier(contact.IdentifierSource, contact.IdentifierValue));
            if(!result.Success)
            {
                // ToDo: Handle error, check result.ErrorCode and result.ErrorMessage for more details
            }
        }

        public void UpdateContactFacet<T>(ContactIdentifier contactIdentifier, string facetKey, Action<T> updateFacets) where T : Facet, new()
        {
            UpdateContactFacet(contactIdentifier, facetKey, updateFacets, () => new T());
        }

        public void UpdateContactFacet<T>(
            ContactIdentifier contactIdentifier,
            string facetKey,
            Action<T> updateFacets,
            Func<T> createFacet)
            where T : Facet
        {
            if (contactIdentifier == null)
            {
                throw new ArgumentNullException(nameof(contactIdentifier));
            }

            _xConnectContactRepository.UpdateContactFacet(new IdentifiedContactReference(contactIdentifier.Source, contactIdentifier.Identifier), new ContactExpandOptions(facetKey), updateFacets, createFacet);
        }

        public void UpdateOrCreateContact(IXConnectContactWithEmail contact)
        {
            CheckIdentifier(contact);
            _xConnectContactRepository.UpdateOrCreateXConnectContactWithEmail(contact);
        }

        private static ContactIdentifier GetValueFromEmailAddressListFacet(EmailAddressList facet)
        {
            var preferredEmail = facet.PreferredEmail;
            var smtpAddress = preferredEmail?.SmtpAddress;

            return !string.IsNullOrEmpty(smtpAddress) ? new ContactIdentifier(ContactIdentifiers.Email, facet.PreferredEmail.SmtpAddress, ContactIdentifierType.Known) : null;
        }
    }
}
