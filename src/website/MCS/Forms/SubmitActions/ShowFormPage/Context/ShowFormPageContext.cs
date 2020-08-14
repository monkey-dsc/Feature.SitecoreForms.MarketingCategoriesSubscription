using System;
using System.Web;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage.Context
{
    // Note: This code is taken from Sitecore Forms Extensions: https://github.com/bartverdonck/Sitecore-Forms-Extensions
    // ToDo: Review this when upgrading to Sitecore 9.3
    public class ShowFormPageContext
    {
        public static Guid? FormPage
        {
            get => (Guid?) HttpContext.Current.Items["NextFormPage"];
            set => HttpContext.Current.Items["NextFormPage"] = value;
        }
    }
}
