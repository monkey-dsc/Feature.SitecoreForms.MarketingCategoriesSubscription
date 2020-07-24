using Feature.SitecoreForms.MarketingCategoriesSubscription.CD.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.CD.Configuration
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by configuration
    public class Configurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IExmSubscriptionClientApiService, ExmSubscriptionClientApiService>();
            serviceCollection.AddTransient<IMarketingPreferencesService, MarketingPreferencesService>();
        }
    }
}
