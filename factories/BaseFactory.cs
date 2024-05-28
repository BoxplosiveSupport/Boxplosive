using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Template;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class BaseFactory
    {
        protected static SelectListItem CreateSelectListItem(string value, string text, bool isSelected = false)
        {
            return new SelectListItem
            {
                Value = value,
                Text = text,
                Selected = isSelected
            };
        }

        protected static SelectListItem CreateSelectListItem<T>(T type, T selected, Func<T, string> valueToText) where T : struct
        {
            return new SelectListItem
            {
                Value = type.Equals(default(T)) ? string.Empty : type.ToString(),
                Text = valueToText(type),
                Selected = selected.Equals(type)
            };
        }

        public static List<SelectListItem> EnumToSelectList<T>(Func<T, string> valueToText, T selected = default(T)) where T : struct
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            return values.Select(v => CreateSelectListItem(v, selected, valueToText)).ToList();
        }

        #region APIs

        protected static IAppCardStackDefinitionApi AppCardStackDefinitionApi = BusinessApiFactory.GetInstance().BusinessApi<IAppCardStackDefinitionApi>();
        protected static ICampaignApi CampaignApi => BusinessApiFactory.GetInstance().BusinessApi<ICampaignApi>();
        protected static ILoyaltyPointsApi LoyaltyPointsApi => BusinessApiFactory.GetInstance().BusinessApi<ILoyaltyPointsApi>();
        protected static ITemplateApi TemplateApi => BusinessApiFactory.GetInstance().BusinessApi<ITemplateApi>();

        #endregion APIs
    }
}