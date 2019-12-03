using System;
using Feature.SitecoreForms.MarketingCategoriesSubscription.Exm.Services.Contact;
using Microsoft.Extensions.DependencyInjection;
using Sitecore.Data.Items;
using Sitecore.DependencyInjection;
using Sitecore.ExperienceForms.Mvc.Models.Fields;
using Sitecore.Framework.Conditions;

namespace Feature.SitecoreForms.MarketingCategoriesSubscription.Forms.FieldTypes
{
    [Serializable]
    // ReSharper disable once UnusedMember.Global
    // Reason: Type is defined in Sitecore
    public class IsKnownContactViewModel : InputViewModel<string>
    {
        private readonly IExmContactService _exmContactService;

        public IsKnownContactViewModel() : this(ServiceLocator.ServiceProvider.GetService<IExmContactService>())
        {
        }

        public IsKnownContactViewModel(IExmContactService exmContactService)
        {
            Condition.Requires(exmContactService, nameof(exmContactService)).IsNotNull();
            _exmContactService = exmContactService;
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

        private bool IsKnownContact()
        {
            return _exmContactService.GetKnownXConnectContactByEmailAddress() != null;
        }

        protected override void UpdateItemFields(Item item)
        {
            base.UpdateItemFields(item);
            var field = item.Fields["Default Value"];
            field?.SetValue(Value, true);
        }
    }
}
