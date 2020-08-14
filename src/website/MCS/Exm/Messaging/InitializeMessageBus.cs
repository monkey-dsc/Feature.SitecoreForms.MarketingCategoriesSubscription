using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.MessageBus;
using Sitecore.EmailCampaign.Model.Web.Settings;
using Sitecore.Framework.Messaging;
using Sitecore.Pipelines;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging
{
    // ReSharper disable once UnusedMember.Global
    // note: initialized by sitecore config
    public class InitializeMessageBus
    {
        private readonly IServiceProvider _serviceProvider;

        public InitializeMessageBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        // ReSharper disable once UnusedMember.Global
        public void Process(PipelineArgs args)
        {
            if (!GlobalSettings.Enabled)
            {
                return;
            }
            _serviceProvider.StartMessageBus<SubscribeContactMessagesBus>();
        }
    }
}
