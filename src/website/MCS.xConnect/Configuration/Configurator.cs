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
            serviceCollection.AddSingleton<IExmContactService, ExmContactService>();

            // XConnect
            serviceCollection.AddSingleton<IXConnectContactFactory, XConnectContactFactory>();
            serviceCollection.AddSingleton<IXConnectContactRepository, XConnectContactRepository>();
            serviceCollection.AddSingleton<IXConnectContactService, XConnectContactService>();
        }
    }
}
