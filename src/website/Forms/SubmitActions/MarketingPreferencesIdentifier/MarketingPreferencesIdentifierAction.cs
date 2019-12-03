using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.MarketingPreferences;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.MarketingPreferencesIdentifier.Base;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.MarketingPreferencesIdentifier.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.EmailCampaign.Cd.Services;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Modules.EmailCampaign.ListManager;
using Sitecore.Modules.EmailCampaign.Services;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.MarketingPreferencesIdentifier
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by custom submit action
    public class MarketingPreferencesIdentifierAction : MarketingPreferencesIdentifierBase<MarketingPreferencesIdentifierData>
    {
        public MarketingPreferencesIdentifierAction(ISubmitActionData submitActionData) : this(
            submitActionData,
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactFactory>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectService>(),
            ServiceLocator.ServiceProvider.GetService<IExmContactService>(),
            ServiceLocator.ServiceProvider.GetService<IExmSubscriptionManager>(),
            ServiceLocator.ServiceProvider.GetService<ICustomMarketingPreferencesService>(),
            ServiceLocator.ServiceProvider.GetService<IClientApiService>(),
            ServiceLocator.ServiceProvider.GetService<IManagerRootService>(),
            ServiceLocator.ServiceProvider.GetService<ListManagerWrapper>())
        {
        }

        public MarketingPreferencesIdentifierAction(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectContactFactory xConnectContactFactory,
            IXConnectService xConnectService,
            IExmContactService exmContactService,
            IExmSubscriptionManager exmSubscriptionManager,
            ICustomMarketingPreferencesService marketingPreferencesService,
            IClientApiService clientApiService,
            IManagerRootService managerRootService,
            ListManagerWrapper listManagerWrapper) : base(submitActionData, logger, xConnectContactFactory, xConnectService, exmContactService, exmSubscriptionManager, marketingPreferencesService, clientApiService, managerRootService, listManagerWrapper)
        {
        }

        protected override ContactIdentifier GetContactIdentifier(MarketingPreferencesIdentifierData identifierData, IEnumerable<IViewModel> fields)
        {
            var fieldList = fields.ToList();

            var emailAddressField = (StringInputViewModel)fieldList.FirstOrDefault(x => x.ItemId == identifierData.FieldEmailAddressId.ToString());
            if (emailAddressField != null && !string.IsNullOrEmpty(emailAddressField.Value))
            {
                return new ContactIdentifier(ContactIdentifiers.Email, emailAddressField.Value, ContactIdentifierType.Known);
            }

            Logger.LogError("Email Address Field is not present.");
            return null;
        }
    }
}
