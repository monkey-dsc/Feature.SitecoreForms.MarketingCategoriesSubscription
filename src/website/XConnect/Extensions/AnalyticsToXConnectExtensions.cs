using System.Collections.Generic;
using System.Linq;
using Sitecore.Analytics.Model;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Extensions
{
    public static class AnalyticsToXConnectExtensions
    {
        public static ContactIdentifier ConvertAnalyticsContactIdentifierToXConnectContactIdentifier(this IEnumerable<Sitecore.Analytics.Model.Entities.ContactIdentifier> contactIdentifiers)
        {
            var identifier = contactIdentifiers.FirstOrDefault(x => x.Type == ContactIdentificationLevel.Known);
            return identifier != null ? new ContactIdentifier(identifier.Source, identifier.Identifier, ContactIdentifierType.Known) : null;
        }
    }
}
