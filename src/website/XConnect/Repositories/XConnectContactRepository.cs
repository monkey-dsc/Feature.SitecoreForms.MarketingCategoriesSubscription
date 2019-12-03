using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;
using Sitecore.Analytics;
using Sitecore.Analytics.Tracking;
using Sitecore.Configuration;
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
                var contact = client.Get(contactReference, new ContactExpandOptions(EmailAddressList.DefaultFacetKey));
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

                MakeContactKnown(client, contact); // ToDo: Review if necessary!

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
                // ToDo: Check how to validate Email in best practice, maybe check the email validation action in Sitecore Forms Extensions how they do that
                facet = new EmailAddressList(new EmailAddress(xConnectContact.Email, false), "Preferred");
            }
            else
            {
                if (facet.PreferredEmail?.SmtpAddress == xConnectContact.IdentifierValue)
                {
                    return;
                }

                // ToDo: Check how to validate Email in best practice, maybe check the email validation action in Sitecore Forms Extensions how they do that
                facet.PreferredEmail = new EmailAddress(xConnectContact.Email, false);
            }

            client.SetEmails(contact, facet);
        }

        public void ReloadContactDataIntoSession()
        {
            if (Tracker.Current?.Contact == null)
            {
                return;
            }

            if (!(CreateContactManager() is ContactManager manager))
            {
                return;
            }

            manager.RemoveFromSession(Tracker.Current.Contact.ContactId);
            Tracker.Current.Session.Contact = manager.LoadContact(Tracker.Current.Contact.ContactId);
        }


        private static object CreateContactManager()
        {
            return Factory.CreateObject("tracking/contactManager", true);
        }

        private static void MakeContactKnown(IXdbContext client, Contact contact)
        {
            if (contact.IsKnown)
            {
                return;
            }

            client.AddContactIdentifier(contact, new ContactIdentifier("xDB.Tracker", Guid.NewGuid().ToString("N"), ContactIdentifierType.Known));
        }
    }
}
