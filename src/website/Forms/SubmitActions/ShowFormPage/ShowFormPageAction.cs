using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage.Context;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage.Data;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Processing;
using Sitecore.ExperienceForms.Processing.Actions;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by custom submit action
    public class ShowFormPageAction : SubmitActionBase<ShowFormPageData>
    {
        private readonly ILogger _logger;

        public ShowFormPageAction(ISubmitActionData submitActionData) : this(submitActionData, ServiceLocator.ServiceProvider.GetService<ILogger>())
        {
        }

        public ShowFormPageAction(ISubmitActionData submitActionData, ILogger logger) : base(submitActionData)
        {
            _logger = logger;
        }

        protected override bool Execute(ShowFormPageData data, FormSubmitContext formSubmitContext)
        {
            if (data.FormPageId == null || data.FormPageId == Guid.Empty)
            {
                _logger.LogWarn("Empty FormPageId");
                return true; //we will not crash on this
            }
            ShowFormPageContext.FormPage = data.FormPageId;
            return true;
        }

    }
}
