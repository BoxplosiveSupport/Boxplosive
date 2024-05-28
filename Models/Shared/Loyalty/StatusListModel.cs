using System;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class StatusListModel : StepModelBase
    {
        public StatusListModel()
        {
            ErrorMessages = new Dictionary<string, LoyaltySteps>();
        }

        public bool? Basics { get; set; }
        public bool? Timeframe { get; set; }
        public bool? Rewards { get; set; }
        public bool? Savings { get; set; }

        public void ValidateLoyaltyAndSetStepStatuses(LoyaltyModel loyaltyModel)
        {
            ValidateLoyaltyModel(loyaltyModel);
            SetStepStatuses(loyaltyModel);
        }

        private void ValidateLoyaltyModel(LoyaltyModel loyaltyModel)
        {
            var timeFrameModel = loyaltyModel.Timeframe;
            var rewardsStepModel = loyaltyModel.Rewards;
            var savingsStepModel = loyaltyModel.Savings;
            
            if (rewardsStepModel.RewardRules.Any())
            {
                var minRewardStartDate = rewardsStepModel.RewardRules.Min(r => r.StartsAtDate);
                var maxRewardEndDate = rewardsStepModel.RewardRules.Min(r => r.EndsAtDate);

                if (timeFrameModel.StartDate > minRewardStartDate)
                {
                    this.ErrorMessages.Add(
                            String.Format("Timeframe cannot start after the first rewards rule starts ({0})", minRewardStartDate.ToString("dd-MM-yyyy")),
                            LoyaltySteps.Timeframe | LoyaltySteps.Rewards
                        );
                }

                if (!loyaltyModel.Basics.IsContinuousSaving && maxRewardEndDate < timeFrameModel.EndDate)
                {
                    this.ErrorMessages.Add(
                            String.Format("Timeframe, for a non-continuous saving program, cannot end after the last rewards rule ends ({0})", maxRewardEndDate.ToString("dd-MM-yyyy")),
                            LoyaltySteps.Timeframe | LoyaltySteps.Rewards
                        );
                }
            }

            if (savingsStepModel.SavingRules.Any())
            {
                var minSavingStartDate = savingsStepModel.SavingRules.Min(s => s.StartDate);
                var maxSavingEndDate = savingsStepModel.SavingRules.Max(s => s.EndDate);

                if (timeFrameModel.StartDate > minSavingStartDate)
                {
                    this.ErrorMessages.Add(
                            String.Format("Timeframe cannot start after the first savings rule starts ({0})", minSavingStartDate.ToString("dd-MM-yyyy")),
                            LoyaltySteps.Timeframe | LoyaltySteps.Savings
                        );
                }

                if (maxSavingEndDate > timeFrameModel.EndDate)
                {
                    this.ErrorMessages.Add(
                            String.Format("Timeframe cannot end before the last savings rule ends ({0})", maxSavingEndDate.ToString("dd-MM-yyyy")),
                            LoyaltySteps.Timeframe | LoyaltySteps.Savings
                        );
                }
            }
        }

        private void SetStepStatuses(LoyaltyModel loyaltyModel)
        {
            Basics = loyaltyModel.Basics.IsValid;
            Timeframe = loyaltyModel.Timeframe.IsValid;
            Rewards = loyaltyModel.Rewards.IsValid;
            Savings = loyaltyModel.Savings.IsValid;
        }

        public string BasicsStyle
        {
            get
            {
                if (Basics.HasValue)
                {
                    if (Basics.Value && !ErrorMessages.Any(e => e.Value.HasFlag(LoyaltySteps.Basics)))
                    {
                        return "text-success";
                    }
                    else
                    {
                        return "text-danger";
                    }
                }
                return "";
            }
        }

        public string TimeframeStyle
        {
            get
            {
                if (Timeframe.HasValue)
                {
                    if (Timeframe.Value && !ErrorMessages.Any(e => e.Value.HasFlag(LoyaltySteps.Timeframe)))
                    {
                        return "text-success";
                    }
                    else
                    {
                        return "text-danger";
                    }
                }
                return "";
            }
        }

        public string RewardsStyle
        {
            get
            {
                if (Rewards.HasValue)
                {
                    if (Rewards.Value && !ErrorMessages.Any(e => e.Value.HasFlag(LoyaltySteps.Rewards)))
                    {
                        return "text-success";
                    }
                    else
                    {
                        return "text-danger";
                    }
                }
                return "";
            }
        }

        public string SavingsStyle
        {
            get
            {
                if (Savings.HasValue)
                {
                    if (Savings.Value && !ErrorMessages.Any(e => e.Value.HasFlag(LoyaltySteps.Savings)))
                    {
                        return "text-success";
                    }
                    else
                    {
                        return "text-danger";
                    }
                }
                return "";
            }
        }

        public string PublishStyle
        {
            get
            {
                if (ReadyToPublish)
                {
                    return "";
                }

                return "disabled";
            }
        }

        public bool ReadyToPublish
        {
            get
            {
                return Basics.HasValue && Basics.Value &&
                     Timeframe.HasValue && Timeframe.Value &&
                     Rewards.HasValue && Rewards.Value &&
                     Savings.HasValue && Savings.Value &&
                     !ErrorMessages.Any();
            }
        }

        public Dictionary<string, LoyaltySteps> ErrorMessages { get; set; }
    }
}