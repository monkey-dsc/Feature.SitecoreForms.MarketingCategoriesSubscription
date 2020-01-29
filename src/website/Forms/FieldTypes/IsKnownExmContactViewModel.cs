using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.XConnect.Services;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Analytics;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes
{
    [Serializable]
    public class IsKnownContactViewModel : InputViewModel<string>
    {
        private readonly IXConnectContactService _xConnectContactService;

        public IsKnownContactViewModel() : this(ServiceLocator.ServiceProvider.GetService<IXConnectContactService>())
        {
        }

        public IsKnownContactViewModel(IXConnectContactService xConnectContactService)
        {
            Condition.Requires(xConnectContactService, nameof(xConnectContactService)).IsNotNull();
            _xConnectContactService = xConnectContactService;
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
            if (Tracker.Current == null || Tracker.Current.Contact == null || Tracker.Current.Contact.IsNew)
            {
                return false;
            }

            return _xConnectContactService.GetXConnectContactByEmailAddress() != null;
        }
    }
}
