using System;
using System.Web;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage.Context
{
    public class ShowFormPageContext
    {
        public static Guid? FormPage
        {
            get => (Guid?) HttpContext.Current.Items["NextFormPage"];
            set => HttpContext.Current.Items["NextFormPage"] = value;
        }
    }
}
