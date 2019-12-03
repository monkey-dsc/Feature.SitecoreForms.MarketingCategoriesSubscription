using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories
{
    public interface IXConnectContactFactory
    {
        IXConnectContact CreateContact(string identifierValue);
        IXConnectContactWithEmail CreateContactWithEmail(string email);
    }
}
