using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Models;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Factories
{
    public interface IXConnectContactFactory
    {
        IXConnectContact CreateContact(string identifierValue);
        IXConnectContactWithEmail CreateContactWithEmail(string email);
    }
}
