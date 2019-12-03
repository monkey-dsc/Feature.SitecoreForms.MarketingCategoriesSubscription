using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories
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
