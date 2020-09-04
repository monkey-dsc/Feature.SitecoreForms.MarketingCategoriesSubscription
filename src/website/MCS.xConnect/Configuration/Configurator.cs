using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Repositories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Configuration
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by configuration
    public class Configurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddTransient<IExmContactService, ExmContactService>();

            // XConnect
            serviceCollection.AddTransient<IXConnectContactFactory, XConnectContactFactory>();
            serviceCollection.AddTransient<IXConnectContactRepository, XConnectContactRepository>();
            serviceCollection.AddTransient<IXConnectContactService, XConnectContactService>();
        }
    }
}
