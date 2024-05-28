using System.Collections.Generic;
using System.ComponentModel;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class RewardsStepModel : StepModelBase
    {
        public RewardsStepModel()
        {
            RewardRules = new List<RewardsRuleModel>();
        }

        [DisplayName("Add rewards rule")]
        public DiscountRuleType RuleType { get; set; }

        [AtLeastOneItem(minElements: 1, ErrorMessage = "Add at least 1 reward rule")]
        public List<RewardsRuleModel> RewardRules { get; set; }

        public RewardsRuleModel SelectedRule { get; set; }

        [DisplayName("Combine rewards")]
        public bool? CombineRewards { get; set; }

        public bool RewardActivation { get; set; }

        public string GetRewardActivationLabelText(bool isContinuousSaving)
        {
            return isContinuousSaving ? "Saving goal activation" : "Reward activation";
        }

        public bool IsSubsequentCombinedReward(RewardsRuleModel selectedRule = null)
        {
            selectedRule = selectedRule ?? SelectedRule;
            if (selectedRule == null)
            {
                return false;
            }

            if (selectedRule.IsSubsequentCombinedReward == true)
            {
                return true;
            }

            var rewardRules = RewardRules ?? new List<RewardsRuleModel>();
            selectedRule.IsSubsequentCombinedReward = CombineRewards == true
                && rewardRules.Count > 0
                && rewardRules.IndexOf(selectedRule) != 0;

            return selectedRule.IsSubsequentCombinedReward.Value;
        }
    }
}