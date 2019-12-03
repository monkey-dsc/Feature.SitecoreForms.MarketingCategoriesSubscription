using Sitecore;
using Sitecore.Mvc.Helpers;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Helper
{
    public static class HtmlHelper
    {
        public static bool IsExperienceFormsEditMode(this SitecoreHelper helper)
        {
            return Context.Request.QueryString["sc_formmode"] != null;
        }
    }
}
