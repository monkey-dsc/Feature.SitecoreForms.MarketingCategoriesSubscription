using System.Collections.Generic;
using System.Linq;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Constants;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Contract.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Base;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Data;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.SubmitActions.SaveMarketingPreferences.Services;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Factories;
using Feature.SitecoreForms.MarketingCategoriesSubscription.xConnect.Services;
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
        private readonly IXConnectContactService _xConnectContactService;

        public SaveMarketingPreferencesAction(ISubmitActionData submitActionData)
            : this(
            submitActionData,
            ServiceLocator.ServiceProvider.GetService<ILogger>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactService>(),
            ServiceLocator.ServiceProvider.GetService<IXConnectContactFactory>(),
            ServiceLocator.ServiceProvider.GetService<ISaveMarketingPreferencesService<SaveMarketingPreferencesData>>(),
            ServiceLocator.ServiceProvider.GetService<IMarketingPreferencesService>(),
            ServiceLocator.ServiceProvider.GetService<IExmSubscriptionClientApiService>())
        {
        }

        public SaveMarketingPreferencesAction(
            ISubmitActionData submitActionData,
            ILogger logger,
            IXConnectContactService xConnectContactService,
            IXConnectContactFactory xConnectContactFactory,
            ISaveMarketingPreferencesService<SaveMarketingPreferencesData> saveMarketingPreferencesService,
            IMarketingPreferencesService marketingPreferencesService,
            IExmSubscriptionClientApiService exmSubscriptionClientApiService)
            : base(submitActionData, logger, xConnectContactService, xConnectContactFactory, saveMarketingPreferencesService, marketingPreferencesService, exmSubscriptionClientApiService)
        {
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            _xConnectContactService = xConnectContactService;
        }

        protected override ContactIdentifier GetContactIdentifier(SaveMarketingPreferencesData data, IEnumerable<IViewModel> fields)
        {
            var fieldList = fields.ToList();

            var emailAddressField = (StringInputViewModel)fieldList.FirstOrDefault(x => x.ItemId == data.FieldEmailAddressId.ToString());
            if (emailAddressField != null && !string.IsNullOrEmpty(emailAddressField.Value))
            {
                return new ContactIdentifier(ContactIdentifiers.Email, emailAddressField.Value, ContactIdentifierType.Known);
            }

            return _xConnectContactService.GetEmailContactIdentifierOfCurrentContact();
        }
    }
}
