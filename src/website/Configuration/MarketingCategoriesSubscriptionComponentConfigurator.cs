using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.MarketingPreferences;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Repositories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Configuration
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by configuration
    public class MarketingCategoriesSubscriptionComponentConfigurator : IServicesConfigurator
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            // EXM
            serviceCollection.AddSingleton<IExmSubscriptionManager, ExmSubscriptionManager>();
            serviceCollection.AddSingleton<IExmContactService, ExmContactService>();
            serviceCollection.AddSingleton<ICustomMarketingPreferencesService, CustomMarketingPreferencesService>();

            // Forms
            serviceCollection.AddSingleton<ISaveMarketingPreferencesService<SaveMarketingPreferencesData>, SaveMarketingPreferencesService<SaveMarketingPreferencesData>>();

            // XConnect
            serviceCollection.AddSingleton<IXConnectContactFactory, XConnectContactFactory>();
            serviceCollection.AddSingleton<IXConnectContactRepository, XConnectContactRepository>();
            serviceCollection.AddSingleton<IXConnectService, XConnectService>();
        }
    }
}
