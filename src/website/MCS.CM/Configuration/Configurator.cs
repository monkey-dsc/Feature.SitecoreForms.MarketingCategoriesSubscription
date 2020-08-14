using Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Messaging;
using Feature.SitecoreForms.MarketingCategoriesSubscription.CM.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

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
        }
    }
}
