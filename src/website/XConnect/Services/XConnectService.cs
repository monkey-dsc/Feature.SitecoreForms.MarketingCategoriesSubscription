using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Extensions;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Analytics.XConnect.Facets;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Model.XConnect.Facets;
using Sitecore.Framework.Conditions;
using Sitecore.XConnect;
using Sitecore.XConnect.Collection.Model;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services
{
    internal sealed class XConnectService : IXConnectService
    {
        private readonly IExmContactService _exmContactService;
        private readonly IXConnectContactRepository _xConnectContactRepository;

        private const double Delay = 100;
        private const int RetryCount = 3;

        public XConnectService() : this(
            ServiceLocator.ServiceProvider.GetService<IExmContactService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactRepository>())
        {
        }

        public XConnectService(IExmContactService exmContactService, IXConnectContactRepository xConnectContactRepository)
        {
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            Condition.Requires(xConnectContactRepository, nameof(xConnectContactRepository)).IsNotNull();
            _exmContactService = exmContactService;
            _xConnectContactRepository = xConnectContactRepository;
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

            if (Tracker.Current.Session != null)
            {
                Tracker.Current.Session.IdentifyAs(contact.IdentifierSource, contact.IdentifierValue);
            }
        }

        public void ReloadContactDataIntoSession()
        {
            _xConnectContactRepository.ReloadContactDataIntoSession();
        }

        public void UpdateCurrentContactFacet<T>(string facetKey, Action<T> updateFacets) where T : Facet, new()
        {
            UpdateCurrentContactFacet(facetKey, updateFacets, () => new T());
        }

        public void UpdateCurrentContactFacet<T>(string facetKey, Action<T> updateFacets, Func<T> createFacet) where T : Facet
        {
            if (Tracker.Current == null || Tracker.Current.Contact == null)
            {
                return;
            }

            if (Tracker.Current.Contact.IsNew)
            {
                _xConnectContactRepository.SaveNewContactToCollectionDb(Tracker.Current.Contact);
            }

            var trackerIdentifier = new IdentifiedContactReference(Sitecore.Analytics.XConnect.DataAccess.Constants.IdentifierSource, Tracker.Current.Contact.ContactId.ToString("N"));
            _xConnectContactRepository.UpdateContactFacet(trackerIdentifier, new ContactExpandOptions(facetKey), updateFacets, createFacet);
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
