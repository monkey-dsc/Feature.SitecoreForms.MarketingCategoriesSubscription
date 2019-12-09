using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Configuration;
using Sitecore.XConnect.Collection.Model;
using Contact = Sitecore.XConnect.Contact;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories
{
    internal class XConnectContactRepository : IXConnectContactRepository
    {
        public void UpdateOrCreateXConnectContactWithEmail(IXConnectContactWithEmail xConnectContact)
        {
            if (xConnectContact == null)
            {
                throw new ArgumentNullException(nameof(xConnectContact));
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                var contactReference = new IdentifiedContactReference(xConnectContact.IdentifierSource, xConnectContact.IdentifierValue);
                var contact = client.Get(contactReference, new ContactExpandOptions(PersonalInformation.DefaultFacetKey, EmailAddressList.DefaultFacetKey));
                if (contact == null)
                {
                    var newContact = new Contact(new ContactIdentifier(contactReference.Source, contactReference.Identifier, ContactIdentifierType.Known));
                    SetEmail(newContact, xConnectContact, client);
                    client.AddContact(newContact);
                    client.Submit();
                }
                else
                {
                    if (contact.Emails()?.PreferredEmail.SmtpAddress == xConnectContact.IdentifierValue)
                    {
                        return;
                    }

                    SetEmail(contact, xConnectContact, client);
                    client.Submit();
                }
            }
        }
        
        public void UpdateContactFacet<T>(
            IdentifiedContactReference reference,
            ContactExpandOptions expandOptions,
            Action<T> updateFacets,
            Func<T> createFacet)
            where T : Facet
        {
            if (reference == null)
            {
                throw new ArgumentNullException(nameof(reference));
            }

            if (expandOptions == null)
            {
                throw new ArgumentNullException(nameof(expandOptions));
            }

            using (var client = SitecoreXConnectClientConfiguration.GetClient())
            {
                var contact = client.Get(reference, expandOptions);
                if (contact == null)
                {
                    return;
                }

                MakeContactKnown(client, contact);

                var facet = contact.GetFacet<T>() ?? createFacet();
                var data = facet;
                updateFacets(data);
                client.SetFacet(contact, data);
                client.Submit();
            }
        }

        private static void SetEmail(Contact contact, IXConnectContactWithEmail xConnectContact, IXdbContext client)
        {
            if (string.IsNullOrEmpty(xConnectContact.Email))
            {
                return;
            }

            var facet = contact.Emails();
            if (facet == null)
            {
                facet = new EmailAddressList(new EmailAddress(xConnectContact.Email, false), "Preferred");
            }
            else
            {
                if (facet.PreferredEmail?.SmtpAddress == xConnectContact.IdentifierValue)
                {
                    return;
                }

                facet.PreferredEmail = new EmailAddress(xConnectContact.Email, false);
            }

            client.SetEmails(contact, facet);
        }

        private static void MakeContactKnown(IXdbContext client, Contact contact)
        {
            if (contact.IsKnown)
            {
                return;
            }

            client.AddContactIdentifier(contact, new ContactIdentifier("exm-known", Guid.NewGuid().ToString("N"), ContactIdentifierType.Known));
        }
    }
}
