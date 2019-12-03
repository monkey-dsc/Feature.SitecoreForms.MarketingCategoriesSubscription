using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.Framework.Conditions;
using Sitecore.Modules.EmailCampaign.Core.Contacts;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services
{
    internal sealed class XConnectService : IXConnectService
    {
        private readonly IContactService _contactService;
        private readonly IXConnectContactRepository _xConnectContactRepository;

        private const double Delay = 100;
        private const int RetryCount = 3;

        public XConnectService() : this(
            ServiceLocator.ServiceProvider.GetService<IContactService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactRepository>())
        {
        }

        public XConnectService(IContactService contactService, IXConnectContactRepository xConnectContactRepository)
        {
            Condition.Requires(contactService, nameof(contactService)).IsNotNull();
            Condition.Requires(xConnectContactRepository, nameof(xConnectContactRepository)).IsNotNull();
            _contactService = contactService;
            _xConnectContactRepository = xConnectContactRepository;
        }

        public Contact GetXConnectContact(ContactIdentifier contactIdentifier, params string[] facetKeys)
        {
            var contactWithRetry = _contactService.GetContactWithRetry(contactIdentifier, Delay, RetryCount, facetKeys);
            return contactWithRetry;
        }

        public void UpdateOrCreateContact(IXConnectContactWithEmail contact)
        {
            CheckIdentifier(contact);
            _xConnectContactRepository.UpdateOrCreateXConnectContactWithEmail(contact);
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

        private static void CheckIdentifier(IXConnectContact contact)
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
    }
}
