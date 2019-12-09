using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.ShowFormPage.Context;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc;
using Sitecore.ExperienceForms.Mvc.Pipelines.RenderForm;
using Sitecore.Mvc.Pipelines;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.Pipelines.ShowFormPage
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by configuration
    // Note: This code is taken from Sitecore Forms Extensions: https://github.com/bartverdonck/Sitecore-Forms-Extensions
    // ToDo: Review this when upgrading to Sitecore 9.3
    public class ShowFormPageNavigationLoader : MvcPipelineProcessor<RenderFormEventArgs>
    {
        private readonly IFormRenderingContext _formRenderingContext;

        public ShowFormPageNavigationLoader(IFormRenderingContext formRenderingContext)
        {
            _formRenderingContext = formRenderingContext;
        }

        public override void Process(RenderFormEventArgs args)
        {
            var page = ShowFormPageContext.FormPage;
            if (!page.HasValue || page.Value == Guid.Empty)
            {
                return;
            }
            _formRenderingContext.NavigationData = new NavigationData
            {
                PageId = page.Value.ToString(),
                Step = 0,
                NavigationType = NavigationType.Submit
            };
        }
    }
}
