using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Models;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Factories
{
    public class XConnectContactFactory : IXConnectContactFactory
    {
        public IXConnectContact CreateContact(string identifierValue)
        {
            return CreateContactWithEmail(identifierValue);
        }

        public IXConnectContactWithEmail CreateContactWithEmail(string email)
        {
            return new XConnectContactWithEmail(email);
        }
    }
}
