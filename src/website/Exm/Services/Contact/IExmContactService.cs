namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact
{
    public interface IExmContactService
    {
        Sitecore.XConnect.Contact GetKnownXConnectContactByEmailAddress();

        void EnsureExmKeyBehaviorCache(Sitecore.XConnect.Contact contact);
    }
}
