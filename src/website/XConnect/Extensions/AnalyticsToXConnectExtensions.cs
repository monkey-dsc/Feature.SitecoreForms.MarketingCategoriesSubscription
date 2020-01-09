using System.Collections.Generic;
using System.Linq;
using Sitecore.Analytics.Model;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Extensions
{
    public static class AnalyticsToXConnectExtensions
    {
        public static ContactIdentifier AnalyticsContactIdentifierToXConnectContactIdentifierByKnown(this IEnumerable<Sitecore.Analytics.Model.Entities.ContactIdentifier> contactIdentifiers)
        {
            var identifier = contactIdentifiers.FirstOrDefault(x => x.Type == ContactIdentificationLevel.Known);
            return identifier != null ? new ContactIdentifier(identifier.Source, identifier.Identifier, ContactIdentifierType.Known) : null;
        }

        public static ContactIdentifier AnalyticsContactIdentifierToXConnectContactIdentifierBySource(this IEnumerable<Sitecore.Analytics.Model.Entities.ContactIdentifier> contactIdentifiers, string source)
        {
            var identifier = contactIdentifiers.FirstOrDefault(x => x.Source == source);
            return identifier != null ? new ContactIdentifier(identifier.Source, identifier.Identifier, ContactIdentifierType.Known) : null;
        }
    }
}
