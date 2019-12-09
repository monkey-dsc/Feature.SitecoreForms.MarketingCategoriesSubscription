using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes
{
    [Serializable]
    public class IsKnownContactViewModel : InputViewModel<string>
    {
        private readonly IXConnectService _xConnectService;

        public IsKnownContactViewModel() : this(ServiceLocator.ServiceProvider.GetService<IXConnectService>())
        {
        }

        public IsKnownContactViewModel(IXConnectService xConnectService)
        {
            Condition.Requires(xConnectService, nameof(xConnectService)).IsNotNull();
            _xConnectService = xConnectService;
        }

        protected override void InitializeValue(object value)
        {
            var str = value?.ToString();
            Value = str;
        }

        protected override void InitItemProperties(Item item)
        {
            base.InitItemProperties(item);
            Value = IsKnownContact().ToString();
        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            var field = item.Fields["Default Value"];
            field?.SetValue(Value, true);
        }

        private bool IsKnownContact()
        {
            return _xConnectService.GetXConnectContactByEmailAddress() != null;
        }
    }
}
