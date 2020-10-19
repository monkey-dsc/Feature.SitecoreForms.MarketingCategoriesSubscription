using Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging;
using Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Cd.Services;
using IMarketingPreferencesService = Sitecore.EmailCampaign.Cd.Services.IMarketingPreferencesService;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Configuration
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by configuration
    public class Configurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IExmSubscriptionClientApiService, ExmStandaloneSubscriptionClientApiService>();
            serviceCollection.AddTransient<ISubscribeContactService, SubscribeContactService>();

            /*  ToDo:
                Dirty workaround, the Sitecore.EmailCampaign.Cd.Services.MarketingPreferencesService is configured to
                be only loaded on role "Standalone or ContentDelivery". If the instance is configured to "ContentManagement" only,
                the Module will throw a NullReferenceException.
                https://github.com/monkey-dsc/Feature.SitecoreForms.MarketingCategoriesSubscription/issues Issue #5
            */
            serviceCollection.AddTransient<IMarketingPreferencesService, MarketingPreferencesService>();
        }
    }
}
