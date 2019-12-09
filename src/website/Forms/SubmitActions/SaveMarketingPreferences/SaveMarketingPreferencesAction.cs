using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Managers;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.MarketingPreferences;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Base;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.DependencyInjection;
using Sitecore.ExM.Framework.Diagnostics;
using Sitecore.ExperienceForms.Models;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;
using Sitecore.XConnect;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences
{
    // ReSharper disable once UnusedMember.Global
    // Reason: Used by custom submit action
    public class SaveMarketingPreferencesAction : SaveMarketingPreferencesBase<SaveMarketingPreferencesData>
    {
        private readonly IXConnectService _xConnectService;

        public SaveMarketingPreferencesAction(ISubmitActionData submitActionData) : this(
            submitActionData,
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactFactory>(),
            ServiceLocator.ServiceProvider.GetService<ISaveMarketingPreferencesService<SaveMarketingPreferencesData>>(),
            ServiceLocator.ServiceProvider.GetService<ICustomMarketingPreferencesService>(),
            ServiceLocator.ServiceProvider.GetService<IExmSubscriptionManager>())
        {
        }

        public SaveMarketingPreferencesAction(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectService xConnectService,
            IXConnectContactFactory xConnectContactFactory,
            ISaveMarketingPreferencesService<SaveMarketingPreferencesData> saveMarketingPreferencesService,
            ICustomMarketingPreferencesService marketingPreferencesService,
            IExmSubscriptionManager exmSubscriptionManager) : base(submitActionData, logger, xConnectService, xConnectContactFactory, saveMarketingPreferencesService, marketingPreferencesService, exmSubscriptionManager)
        {
            Condition.Requires(xConnectService, nameof(xConnectService)).IsNotNull();
            _xConnectService = xConnectService;
        }

        protected override ContactIdentifier GetContactIdentifier(SaveMarketingPreferencesData data, IEnumerable<IViewModel> fields)
        {
            var fieldList = fields.ToList();

            var emailAddressField = (StringInputViewModel)fieldList.FirstOrDefault(x => x.ItemId == data.FieldEmailAddressId.ToString());
            if (emailAddressField != null && !string.IsNullOrEmpty(emailAddressField.Value))
            {
                return new ContactIdentifier(ContactIdentifiers.Email, emailAddressField.Value, ContactIdentifierType.Known);
            }

            return _xConnectService.GetEmailContactIdentifierOfCurrentContact();
        }
    }
}
