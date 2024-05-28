using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Configuration;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class SavingsStepModel : StepModelBase
    {
        public SavingsStepModel()
        {
            SavingRules = new List<SavingsRuleModel>();
        }

        [DisplayName("Add savings rule")]
        public SavingsRuleTypeModel? RuleType { get; set; }

        /// <remarks>
        /// Note that always the "make a choice" option is selected (never one of the other options) when the drop-down is shown.
        /// </remarks>
        public IList<SelectListItem> RuleTypes => System.Web.Mvc.Html.EnumHelper.GetSelectList(typeof(SavingsRuleTypeModel))
            .Where(item => int.Parse(item.Value) != (int)SavingsRuleTypeModel.CodeActionSaving || AppConfig.Settings.Feature_QrSaving)
            .ToList();

        [AtLeastOneItem]
        public List<SavingsRuleModel> SavingRules { get; set; }

        public SavingsRuleModel SelectedRule { get; set; }

        public bool IsPublished { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = ValidationHelper.ValueCannotBeNegative)]
        [DisplayName("Max points for anonymous accounts")]
        [DisplayFormat(NullDisplayText = "Unlimited")]
        public int? AnonymousBalanceLimit { get; set; }

        [DisplayName("Display type")]
        [Required]
        public SavingsDisplayTypeModel SavingsDisplayType { get; set; }

        public List<SelectListItem> SavingsDisplayTypes { get; set; } =
            BaseFactory.EnumToSelectList((SavingsDisplayTypeModel value) => value.ToString());
    }
}