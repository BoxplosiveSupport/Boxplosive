using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Service.ServiceContract.FileService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Campaign;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public abstract class LoyaltyControllerBase<T> : PlatformController where T : ILoyaltyModel
    {
        protected abstract LoyaltyCampaign GetLoyaltyFromCache();

        protected abstract void SaveLoyaltyInCache(LoyaltyCampaign campaign);

        protected abstract T GetLoyaltyModelFromCache();

        protected abstract void StoreLoyaltyModelInCache(T model);

        public bool IsCopy()
        {
            return GetLoyaltyFromCache()?.IsCopy ?? false;
        }

        public bool IsContinuousSaving()
        {
            return GetLoyaltyFromCache()?.IsContinuousSaving ?? false;
        }

        public void SetViewData_IsContinuousSaving()
        {
            ViewData["isContinuousSaving"] = IsContinuousSaving();
        }

        protected void ValidateLoyaltyModelStates(LoyaltyModel model)
        {
            ClearModelStateAndTryValidateModels(new List<StepModelBase> { model.Basics, model.Timeframe });

            ValidateRewardsStepModelStates(model.Rewards);
            ValidateSavingsStepModelStates(model.Savings);
        }

        protected void ValidateRewardsStepModelStates(RewardsStepModel model)
        {
            ClearModelStateAndTryValidateModels(new List<StepModelBase> { model });

            int index = 0;
            foreach (var iModel in model.RewardRules)
            {
                bool isSubsequentCombinedReward = model.CombineRewards == true && model.RewardRules.Count > 0 && index != 0;
                model.IsValid = (model.IsValid ?? true) && iModel.ErrorMessages.Count == 0
                    && ClearModelStateAndTryValidateModel(iModel, RewardsRuleModel.GetModelStateValidationKeys(isSubsequentCombinedReward));

                index++;
            }
        }

        protected void ValidateSavingsStepModelStates(SavingsStepModel model)
        {
            ClearModelStateAndTryValidateModels(new List<StepModelBase> { model });
            foreach (var iModel in model.SavingRules)
            {
                model.IsValid = (model.IsValid ?? true) && iModel.ErrorMessages.Count == 0
                    && ClearModelStateAndTryValidateModel(iModel, SavingsRuleModel.ModelStateValidationKeys);
            }
        }

        /// <summary>
        /// Copies the shared field values for combined rewards
        /// </summary>
        /// <remarks>
        /// These fields can only be entered in the BackOffice for the first reward rule
        /// and therefore are to be copied to the other reward rules.
        /// </remarks>
        /// <param name="model">The model to update</param>
        protected void CopySharedFieldsForCombinedRewards(RewardsStepModel model)
        {
            RewardsRuleModel firstRuleModel = model.RewardRules.FirstOrDefault();
            if (firstRuleModel == null)
                return;

            foreach (var newRuleModel in model.RewardRules.Skip(1))
                newRuleModel.CopySharedFieldsForCombinedRewards(firstRuleModel);
        }

        protected ActionResult UploadRewardImage()
        {
            var rewardRule = new RewardsRuleModel();
            if (ImageHelper.ValidateAndUpload(out SaveFileResponse uploadResult, Request.Files["SelectedRule.Image"], ModelState))
            {
                rewardRule.ImageUrl = uploadResult.Url;
            }

            return Json(rewardRule);
        }

        protected void ValidateSavingStep(SavingsStepModel model)
        {
            // ELP-6521: for CodeActionSaving (aka QR saving) zero/negative points are not allowed (ignore Feature_AllowNegativePoints)
            //  - Amount field is not used by CodeActionSaving
            if (model.SelectedRule.Type == SavingsRuleTypeModel.CodeActionSaving)
                return;

            if (model.SelectedRule.Amount == 0)
                model.SelectedRule.ErrorMessages.Add(ValidationHelper.ValueCannotBeZero);
            else if (model.SelectedRule.Amount < 0 && !AppConfig.Settings.Feature_AllowNegativePoints)
                model.SelectedRule.ErrorMessages.Add(ValidationHelper.ValueCannotBeNegative);
        }

        protected bool ValidateRedemptionPeriod(string redemptionPeriod)
        {
            TimeSpan? redemptionPeriodTimeSpan;
            bool isValid = TimeSpanExtensions.TryParseCanonical(redemptionPeriod, out redemptionPeriodTimeSpan);
            if (isValid && redemptionPeriodTimeSpan.HasValue)
            {
                isValid = redemptionPeriodTimeSpan.Value != default(TimeSpan);
            }

            return isValid;
        }

        protected bool ValidateBalanceMessages(BasicStepModel model)
        {
            if (!AppConfig.Settings.Feature_BalanceMessages)
                return true;

            bool success = DtoLoyaltyPointsProgram.TryParseBalanceMessages(model.BalanceMessages,
                               out IDictionary<string, MultiLingualMultiFormatString> balanceMessages)
                           && balanceMessages.All(bm => !string.IsNullOrEmpty(bm.Key));

            if (!success)
                ModelState.AddModelError("BalanceMessages", "Incorrect syntax");

            return success;
        }

        [HttpGet]
        public virtual ActionResult DownloadProducts(int selectedRuleIndex, StepType stepType, ProductsType productsType,
            string fileNamePrefix)
        {
            var products = new List<ProductModel>();

            var campaignModel = GetLoyaltyModelFromCache();
            if (campaignModel != null)
            {
                switch (stepType)
                {
                    case StepType.Reward:
                        products = GetRewardProducts(selectedRuleIndex, productsType, campaignModel);
                        break;

                    case StepType.Saving:
                        var savingRule = campaignModel.Loyalty.Savings.SavingRules.ElementAtOrDefault(selectedRuleIndex);
                        if (savingRule != null)
                        {
                            products = savingRule.SelectedProducts;
                        }

                        break;
                }
            }

            return GetDownloadProductsFileResult(products, fileNamePrefix);
        }

        private List<ProductModel> GetRewardProducts(int selectedRuleIndex, ProductsType productsType, T campaignModel)
        {
            RewardsRuleModel rewardRuleModel = campaignModel.Loyalty.Rewards.RewardRules.ElementAtOrDefault(selectedRuleIndex);
            return GetProducts(productsType, rewardRuleModel);
        }

        protected void AssignProductsToRewardRule(RewardsRuleModel rule, LoyaltyModel loyalty, bool isCombinedReward, int? ruleIndex)
        {
            RewardsRuleModel cachedSelectedRule = new RewardsRuleModel();
            if (isCombinedReward && rule.Id > 0)
            {
                cachedSelectedRule = loyalty.Rewards.RewardRules[ruleIndex.Value] ?? new RewardsRuleModel();
            }
            else
            {
                cachedSelectedRule = loyalty.Rewards.RewardRules.FirstOrDefault(r => r.Id == rule.Id) ?? new RewardsRuleModel();
            }

            var modelSelectedRule = rule ?? new RewardsRuleModel();
            LoadProductsFromMultipleProductUpload(modelSelectedRule.Triggers, cachedSelectedRule.Triggers, "Triggers");
            LoadProductsFromMultipleProductUpload(modelSelectedRule.SelectedProducts, cachedSelectedRule.SelectedProducts, "SelectedProducts");
            LoadProductsFromMultipleProductUpload(modelSelectedRule.TargetProducts, cachedSelectedRule.TargetProducts, "TargetProducts");
        }

        #region Placeholder

        public virtual PartialViewResult GetPlaceholders(int? templateId)
        {
            var serviceModel = GetLoyaltyFromCache();
            var model = PlaceholderFactory.GetPlaceholderModels(templateId, serviceModel.PlaceholderValues);

            return PartialView("Placeholders", model);
        }

        public virtual PartialViewResult GetRewardPlaceholders(int? templateId, int? rewardRuleIndex)
        {
            IList<PlaceholderValue> servicePlaceholderValues = new List<PlaceholderValue>();
            if (rewardRuleIndex.HasValue)
            {
                var serviceRewardBase = GetLoyaltyFromCache()?.Rewards.ElementAtOrDefault(rewardRuleIndex.Value);
                if (serviceRewardBase != null)
                    servicePlaceholderValues = serviceRewardBase.PlaceholderValues;
            }

            var model = PlaceholderFactory.GetPlaceholderModels(templateId, servicePlaceholderValues);

            return PartialView("PlaceholdersReward", model);
        }

        #endregion Placeholder

        /// <summary>
        /// Shared rewardRules validator for loyalty campaign.
        /// Only validates against minimum of one existing saving goal at the end of a campaign, for NON continuous campaigns!
        /// Based on requirements from ELP-6901
        /// </summary>
        /// <param name="selectedRule"></param>
        /// <param name="cachedModel"></param>
        public void ValidateRewardRulesWithLoyaltyCampaign(RewardsRuleModel selectedRule, RewardsStepModel cachedModel)
        {
            var campaign = GetLoyaltyFromCache();
            if (!campaign.IsContinuousSaving)
            {
                // Check for errors in the discount rules and return the same view with the appropriate errors
                if (selectedRule != null)
                {
                    RewardsRuleModel rewardRule = cachedModel.RewardRules.First(rr => rr.Id == selectedRule.Id);

                    // ETOS-1418 (2015-12-01): changed requirement.
                    // A least one reward should end on or after the loyalty program
                    bool endsAtIsValid = cachedModel.CombineRewards.HasValue && cachedModel.CombineRewards.Value
                        ? cachedModel.RewardRules[0].EndsAfterLoyaltyCampaign(campaign)
                        : cachedModel.RewardRules.Any(r => r.EndsAfterLoyaltyCampaign(campaign));
                    if (!endsAtIsValid)
                    {
                        ModelState.AddModelError("ErrorMessages", "Without continuous saving, reward end date should be at least campaign end date");
                        rewardRule.ErrorMessages.Add("Without continuous saving, at least 1 reward end date should be the same or greater then the campaign end date");
                    }
                }
                else if (!cachedModel.RewardRules.Any(r => r.EndsAfterLoyaltyCampaign(campaign)))
                {
                    ModelState.AddModelError("RewardRules", "1 or more reward rules are invalid");
                }

                // Check for error to invalidate the reward section in the create loyalty page
                // so it's not possible to complete the creation of the loyalty program
                if (cachedModel.RewardRules.Any(m => m.ErrorMessages.Count > 0))
                {
                    ModelState.AddModelError("RewardRules", "1 or more reward rules are invalid");
                }
            }
        }
    }
}