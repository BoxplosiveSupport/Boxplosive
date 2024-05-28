using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Mapping;
using nl.boxplosive.BackOffice.Mvc.Models.CreateLoyalty;
using nl.boxplosive.BackOffice.Mvc.Models.ModifyLoyalty;
using nl.boxplosive.BackOffice.Mvc.Models.PublishLoyalty;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;
using nl.boxplosive.Configuration;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Campaign;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using StatusListModel = nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty.StatusListModel;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class LoyaltyFactory : BaseFactory
    {
        private static readonly string _StampTypeUnknownText = "Choose stamp type";
        private static readonly string _StampTypeDigitalAndPhysicalText = "Digital and physical";

        public static CreateLoyaltyModel CreateCampaignModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrel,
            string campaignDescription,
            LoyaltyCampaign campaign)
        {
            var model = new CreateLoyaltyModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                StatusList = new StatusListModel(),
                Loyalty = new LoyaltyModel(campaign)
            };

            return model;
        }

        public static PublishLoyaltyModel CreatePublishCampaignModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrel,
            string campaignDescription,
            LoyaltyCampaign campaign)
        {
            var model = new PublishLoyaltyModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                Loyalty = new LoyaltyModel(campaign)
            };

            return model;
        }

        public static ModifyLoyaltyModel CreateModifyCampaignModel(string pageTitle,
           string pageNavigationLabel,
           string pageNavigationUrel,
           string campaignDescription,
           LoyaltyCampaign campaign)
        {
            var model = new ModifyLoyaltyModel
            {
                PageTitle = String.Format("{0} - {1}", pageTitle, campaign.Id),
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                StatusList = new ModifyStatusListModel
                {
                    LoyaltyProgramId = campaign.Id
                },
                LoyaltyProgramId = campaign.Id,
                Loyalty = new LoyaltyModel(campaign)
            };

            return model;
        }

        public static CampaignProcessRequest ConvertCampaignToPublishRequest<T>(LoyaltyCampaign campaign, T process) where T : CampaignProcess
        {
            var request = new CampaignProcessRequest();
            AuthenticationHelpers.SetupServiceRequest(request, "Publish");
            campaign.SavingsType = campaign.SavingsType;
            campaign.StartDate = campaign.StartDate.ToUtcInOperatingTimeZone();
            campaign.EndDate = campaign.EndDate.ToUtcInOperatingTimeZone();

            request.Process = process.Map();
            request.Process.Type = CampaignProcessType.Publish;
            request.Process.Campaign = campaign;

            return request;
        }

        #region Basics Step

        public static string StampTypeToDisplayText(StampType value)
        {
            switch (value)
            {
                case StampType.DigitalAndPhysical:
                    return _StampTypeDigitalAndPhysicalText;

                case StampType.Unknown:
                    return _StampTypeUnknownText;

                default:
                    return value.ToString();
            }
        }

        #endregion Basics Step

        #region Rewards Step

        public static RewardsStepModel CreateRewardsStepModel(LoyaltyCampaign campaign)
        {
            var model = new RewardsStepModel
            {
                RewardRules = new List<RewardsRuleModel>(),
                CombineRewards = campaign.CombineRewards,
                RewardActivation = campaign.RewardActivation
            };
            if (campaign.Rewards != null && campaign.Rewards.Count > 0)
            {
                foreach (var reward in campaign.Rewards.Where(r => r.Status != LoyaltyPointsRewardStatus.Expired))
                {
                    model.RewardRules.Add(new RewardsRuleModel(reward, model.CombineRewards));
                }
            }

            return model;
        }

        public static LoyaltyCampaign UpdateCampaignRewards(LoyaltyCampaign campaign, RewardsStepModel model,
            List<string> selectedUploadProducts = null, List<string> targetUploadProducts = null, List<string> triggerUploadProducts = null)
        {
            if (campaign.Rewards == null)
                campaign.Rewards = new List<RewardBase>();

            // ETOS-1591
            campaign.CombineRewards = model.CombineRewards ?? false;
            campaign.RewardActivation = model.RewardActivation;
            Dictionary<int, LoyaltyPointsRewardStatus> initialRewardStatuses = campaign.Rewards.ToDictionary(r => campaign.Rewards.IndexOf(r), r => r.Status);

            int combinedRewardIndex = 0;
            foreach (var reward in model.RewardRules)
            {
                if (reward.Remove || (reward.Id <= -1 && reward.Disable)) // Remove the items that are removed or that are new and disabled
                {
                    if (campaign.CombineRewards)
                    {
                        campaign.Rewards.RemoveAt(model.RewardRules.IndexOf(reward));
                    }
                    else
                    {
                        campaign.Rewards.RemoveAll(s => s.Id == reward.Id);
                    }
                }
                else
                {
                    RewardBase serviceReward;
                    // Check if the saving is just added
                    if (!reward.New) // update the existing saving
                    {
                        // ETOS-1591: If rewards were combined, retrieve reward using the 'combinedRewardIndex'
                        if (campaign.CombineRewards)
                        {
                            serviceReward = campaign.Rewards
                                .Where(r => initialRewardStatuses[campaign.Rewards.IndexOf(r)] != LoyaltyPointsRewardStatus.Expired)
                                .ElementAt(combinedRewardIndex);
                            combinedRewardIndex++;
                        }
                        else
                        {
                            serviceReward = campaign.Rewards.FirstOrDefault(s => s.Id == reward.Id);
                            if (serviceReward == null)
                            {
                                serviceReward = campaign.Rewards.ElementAt(model.RewardRules.IndexOf(reward));
                            }
                        }
                    }
                    else // Insert a new saving
                    {
                        switch (reward.RewardType)
                        {
                            case LoyaltyPointsRewardType.Claim:
                                {
                                    serviceReward = new ClaimReward
                                    {
                                        Id = reward.Id,
                                        Type = LoyaltyPointsRewardType.Claim.ToString(),
                                        Discount = new DiscountRule
                                        {
                                            Type = reward.Type.ToString(),
                                            ProductIds = new HashSet<string>()
                                        }
                                    };
                                    break;
                                }

                            default:
                                throw new NotImplementedException("Loyalty reward type Unknown is not implemented");
                        }

                        campaign.Rewards.Add(serviceReward);
                    }

                    // Update Claim reward
                    if (reward.RewardType == LoyaltyPointsRewardType.Claim)
                    {
                        var claimReward = (ClaimReward)serviceReward;

                        claimReward.Discount.Triggers = ProductFactory.ProcessProducts(reward.Triggers, triggerUploadProducts);
                        claimReward.Discount.ProductIds = ProductFactory.ProcessProducts(reward.SelectedProducts, selectedUploadProducts);
                        claimReward.Discount.TargetProductIds = ProductFactory.ProcessProducts(reward.TargetProducts, targetUploadProducts);

                        claimReward.Discount.Type = reward.Type.ToString();
                        claimReward.Discount.PayOutLoyaltyProgramId = reward.PayOutLoyaltyProgramId;
                        claimReward.Discount.RequiredNumberOfCoupons = reward.RequiredNumberOfCoupons;
                        claimReward.Discount.MaxNumberOfCouponsPerTransaction = reward.MaxNumberOfCouponsPerTransaction;
                        claimReward.Discount.Amount = reward.Amount;
                        claimReward.Discount.TargetAmount = reward.TargetAmount;
                        claimReward.Discount.RequiredAmount = reward.RequiredAmount;
                        claimReward.Discount.ApplicableAmount = reward.ApplicableAmount;
                        claimReward.Discount.Message = reward.Message;
                        claimReward.Discount.MessageType = reward.MessageType.ToString();
                        claimReward.Discount.MinTransactionValue = reward.MinTransactionValue;
                        claimReward.Discount.MatchMoreUnits = reward.MatchMoreUnits;

                        if (reward.Type == Models.Shared.DiscountRuleType.GrantLoyaltyPoints)
                        {
                            claimReward.Discount.LoyaltyProgramId = reward.GrantPointsLoyaltyProgramId;
                            claimReward.Discount.Amount = reward.NumberOfLoyaltyPoints;
                        }
                    }

                    DateTime todayUtc = OperatingDateTime.TodayUtc;
                    bool todayUtcAfterLoyaltyProgramStart = todayUtc > campaign.StartDate;

                    // Update base values
                    serviceReward.Name = reward.Name;
                    serviceReward.CampaignExternalId = reward.CampaignExternalId;
                    serviceReward.SavingGoalAppCardStackDefinitionId = campaign.IsContinuousSaving ? reward.SavingGoalAppCardStackDefinitionId : (int?)null;
                    serviceReward.SavingGoalAppCardOrder = campaign.IsContinuousSaving ? reward.SavingGoalAppCardOrder : (int?)null;
                    serviceReward.RewardAppCardStackDefinitionId = reward.RewardAppCardStackDefinitionId;
                    serviceReward.RewardAppCardOrder = reward.RewardAppCardOrder;
                    serviceReward.AppCardTemplateId = reward.AppCardTemplateId;
                    serviceReward.ExternalRewardSupplierId = reward.ExternalRewardSupplierId;
                    serviceReward.StockStatus = reward.StockStatus.ConvertFromModel();
                    serviceReward.Threshold = reward.LoyaltyPointTreshold;
                    serviceReward.Delta = reward.LoyaltyPointDelta;
                    serviceReward.Description = reward.Description;
                    serviceReward.ImageFileName = reward.ImageUrl;
                    serviceReward.Status = reward.Disable ? LoyaltyPointsRewardStatus.Expired : LoyaltyPointsRewardStatus.Inactive; // Cron sets status to 'Active' if the time is there
                    serviceReward.StartsAt = reward.StartsAtDate == DateTime.MinValue ? (todayUtcAfterLoyaltyProgramStart ? todayUtc : campaign.StartDate) : reward.StartsAtDate.Add(reward.StartsAtTime).ToUtcInOperatingTimeZone();
                    serviceReward.EndsAt = reward.EndsAtDate == DateTime.MinValue ? campaign.EndDate : reward.EndsAtDate.Add(reward.EndsAtTime).ToUtcInOperatingTimeZone();
                    serviceReward.RepeatableAmount = reward.RepeatableAmount;
                    serviceReward.RedemptionLimit = reward.RedemptionLimitDate.HasValue
                        ? reward.RedemptionLimitDate.Value.Add(reward.RedemptionLimitTime.Value).ToUtcInOperatingTimeZone()
                        : (DateTime?)null;
                    serviceReward.RedemptionPeriod = TimeSpanExtensions.ParseCanonical(reward.RedemptionPeriod);
                    serviceReward.NumberOfRedemptions = reward.MaxNumberOfRedemptions;
                    serviceReward.MailThreshold = reward.MailThreshold;
                    serviceReward.PlaceholderValues = reward.Placeholders.Select(item => item.ToServiceModel()).ToList();

                    if (serviceReward.StartsAt.Equals(serviceReward.EndsAt))
                        serviceReward.EndsAt = serviceReward.EndsAt.AddDays(1);
                }
            }

            return campaign;
        }

        #endregion Rewards Step

        #region Savings Step

        public static SavingsStepModel CreateSavingsStepModel(LoyaltyCampaign campaign)
        {
            var model = new SavingsStepModel
            {
                AnonymousBalanceLimit = campaign.AnonymousBalanceLimit,
                SavingsDisplayType =
                    (SavingsDisplayTypeModel)Enum.Parse(typeof(SavingsDisplayTypeModel), campaign.SavingsDisplayType.ToString())
            };
            if (campaign.Savings != null && campaign.Savings.Count > 0)
            {
                var count = 1;
                foreach (var saving in campaign.Savings.Where(r => r.Status == CampaignStatus.Active || r.Status == CampaignStatus.Inactive))
                {
                    var rule = new SavingsRuleModel(saving);
                    if (rule.Id == 0)
                    {
                        rule.Id = count;
                        count++;
                    }
                    model.SavingRules.Add(rule);
                }
            }

            return model;
        }

        public static LoyaltyCampaign UpdateLoyaltyCampaignSavings(LoyaltyCampaign campaign, SavingsStepModel model, List<string> uploadProducts = null)
        {
            if (campaign.Savings == null)
                campaign.Savings = new List<Saving>();

            campaign.AnonymousBalanceLimit = model.AnonymousBalanceLimit;
            campaign.SavingsDisplayType =
                (SavingsDisplayType)Enum.Parse(typeof(SavingsDisplayType), model.SavingsDisplayType.ToString());

            foreach (var saving in model.SavingRules)
            {
                if (saving.Remove || (saving.Id <= -1 && saving.Disable)) // Remove the items that are removed or that are new and disabled
                {
                    campaign.Savings.RemoveAll(s => s.Discount.Id == saving.Id);
                }
                else
                {
                    Saving serviceSaving;
                    // Check if the saving is just added
                    if (!saving.New) // update the existing saving
                    {
                        serviceSaving = campaign.Savings.First(s => s.Discount.Id == saving.Id);
                    }
                    else // Insert a new saving
                    {
                        serviceSaving = new Saving
                        {
                            Discount = new DiscountRule
                            {
                                Id = saving.Id,
                                Type = saving.Type.ToString(),
                                ProductIds = new HashSet<string>()
                            }
                        };
                        campaign.Savings.Add(serviceSaving);
                    }

                    // Process selected products if there are any
                    if (saving.SelectedProducts != null && saving.SelectedProducts.Count > 0)
                    {
                        serviceSaving.Discount.ProductIds = new HashSet<string>();
                        foreach (var product in saving.SelectedProducts)
                        {
                            // Set the products, hashset auto filters duplicates
                            if (!product.Remove)
                            {
                                serviceSaving.Discount.ProductIds.Add(product.Id);
                            }
                        }

                        saving.SelectedProducts.RemoveAll(sp => sp.Remove);
                    }

                    if (uploadProducts != null && uploadProducts.Count > 0)
                    {
                        if (serviceSaving.Discount.ProductIds == null)
                        {
                            serviceSaving.Discount.ProductIds = new HashSet<string>();
                        }

                        foreach (var uploadProduct in uploadProducts)
                        {
                            serviceSaving.Discount.ProductIds.Add(uploadProduct);
                        }
                    }

                    // Update values
                    serviceSaving.CouponCode = saving.CouponCode;
                    serviceSaving.Name = saving.Name;
                    serviceSaving.RepeatableAmount = saving.RepeatableAmount;
                    serviceSaving.CodeAction = saving.ToCodeActionServiceModel();
                    serviceSaving.Status = saving.Disable ? CampaignStatus.Expired : CampaignStatus.Inactive; // Cron sets status to 'Active' if the time is there
                    serviceSaving.StartsAt = saving.StartDate.Add(saving.From).ToUtcInOperatingTimeZone();
                    serviceSaving.EndsAt = saving.EndDate.Add(saving.To).ToUtcInOperatingTimeZone();
                    serviceSaving.Discount.Amount = saving.Amount;
                    serviceSaving.Discount.RequiredAmount = saving.RequiredAmount;
                    serviceSaving.Discount.ApplicableAmount = saving.ApplicableAmount;
                    serviceSaving.Discount.MinTransactionValue = saving.MinTransactionValue;
                    serviceSaving.Discount.SuppressPointsForRewards = saving.SuppressPointsForRewards;

                    if (saving.Type == SavingsRuleTypeModel.FixedLoyaltyPointsOnTransactionInDaysOfWeek)
                    {
                        serviceSaving.Discount.DaysOfWeek = (boxplosive.Business.Transaction.Rules.Enums.DayRuleChecks)ConvertDaysToDaysOfTheWeek(saving);
                    }
                }
            }

            return campaign;
        }

        #endregion Savings Step

        private static DayRuleChecks ConvertDaysToDaysOfTheWeek(SavingsRuleModel model)
        {
            DayRuleChecks daysOfWeek = DayRuleChecks.None;
            if (model.Monday)
            {
                daysOfWeek |= DayRuleChecks.Monday;
            }
            if (model.Tuesday)
            {
                daysOfWeek |= DayRuleChecks.Tuesday;
            }
            if (model.Wednesday)
            {
                daysOfWeek |= DayRuleChecks.Wednesday;
            }
            if (model.Thursday)
            {
                daysOfWeek |= DayRuleChecks.Thursday;
            }
            if (model.Friday)
            {
                daysOfWeek |= DayRuleChecks.Friday;
            }
            if (model.Saturday)
            {
                daysOfWeek |= DayRuleChecks.Saturday;
            }
            if (model.Sunday)
            {
                daysOfWeek |= DayRuleChecks.Sunday;
            }

            return daysOfWeek;
        }

        public static List<SelectListItem> GetLoyaltyPrograms(int? programId, string defaultOptionText)
        {
            var loyaltyPrograms = new List<SelectListItem>();

            var excluded = new HashSet<int>();
            var included = new HashSet<int>();
            if (programId.HasValue)
                included.Add(programId.Value);

            var programs = LoyaltyPointsApi.GetAllProgramsByStatuses(included, excluded, false,
                LoyaltyPointsProgramStatus.Inactive, LoyaltyPointsProgramStatus.Active, LoyaltyPointsProgramStatus.TemporaryInactive);

            // Add default option
            loyaltyPrograms.Add(CreateSelectListItem(string.Empty, defaultOptionText));
            // Add other options
            var options = programs.OrderBy(item => item.Id).Select(item => CreateSelectListItem(item.Id.ToString(), item.Name, item.Id == programId));
            loyaltyPrograms.AddRange(options);

            return loyaltyPrograms;
        }
    }
}