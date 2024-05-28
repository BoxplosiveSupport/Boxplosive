using System.Collections.Generic;
using System.ComponentModel;
using nl.boxplosive.BackOffice.Mvc.Attributes;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class DiscountStepModel : StepModelBase
    {
        public DiscountStepModel()
        {
            DiscountRules = new List<DiscountRuleModel>();
        }

        [DisplayName("Add discount rule")]
        public DiscountRuleType RuleType { get; set; }

        [AtLeastOneItem(minElements: 1, ErrorMessage = "Add at least 1 discount rule")]
        public List<DiscountRuleModel> DiscountRules { get; set; }

        public DiscountRuleModel SelectedRule { get; set; }

        public bool IsPublished { get; set; }

        public bool IsSubsequentCombinedDiscount(DiscountRuleModel selectedRule = null)
        {
            selectedRule = selectedRule ?? SelectedRule;
            if (selectedRule == null)
            {
                return false;
            }

            if (selectedRule.IsSubsequentCombinedDiscount == true)
            {
                return true;
            }

            var discountRules = DiscountRules ?? new List<DiscountRuleModel>();
            selectedRule.IsSubsequentCombinedDiscount = discountRules.Count > 0
                && discountRules.IndexOf(selectedRule) != 0;

            return selectedRule.IsSubsequentCombinedDiscount.Value;
        }
    }
}