using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Models
{
    public class XConnectContactWithEmail : IXConnectContactWithEmail
    {
        public string IdentifierSource => ContactIdentifiers.Email;
        public string IdentifierValue => Email;

        public string Email { get; set; }

        public XConnectContactWithEmail()
        {
        }

        public XConnectContactWithEmail(string email)
        {
            Email = email;
        }
    }
}
