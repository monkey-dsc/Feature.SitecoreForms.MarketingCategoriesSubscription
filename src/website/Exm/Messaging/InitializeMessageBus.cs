using System;
using Sitecore.EmailCampaign.Model.Web.Settings;
using Sitecore.Framework.Messaging;
using Sitecore.Pipelines;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Messaging
{
    public class InitializeMessageBus
    {
        private readonly IServiceProvider _serviceProvider;

        public InitializeMessageBus(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

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
