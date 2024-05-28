using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public abstract class PromotionControllerBase<T> : PlatformController where T : IPromotionModel
    {
        protected string CacheKey_Retailers
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();
                return $"Retailers-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        protected abstract PromotionCampaign Cache_Get_Promotion();

        protected abstract void Cache_Set_Promotion(PromotionCampaign campaign);

        protected abstract void Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(PromotionCampaign campaign);

        protected abstract void Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState processState);

        protected abstract T Cache_Get_PromotionModel();

        protected abstract void Cache_Set_PromotionModel(T model);

        protected abstract StepModelBase RetrieveModelFromCache(string id);

        protected abstract ActionResult ReturnCorrectView(string id, string mode, StepModelBase model, int? ruleIndex = null);

        protected abstract string ReturnDiscountRuleModelEditorTemplateName();

        #region Step Setters

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepSegmentation(SegmentationStepModel model, string id, string mode)
        {
            // Update the DB draft
            var campaign = Cache_Get_Promotion();
            campaign.NoDistribution = model.SegmentId == model.SegmentId_Private;
            campaign.DistributionId = model.DistributionId;
            campaign.SegmentId = model.SegmentId;

            Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);

            model = RetrieveModelFromCache(id) as SegmentationStepModel;
            //ModelState.Clear();
            //TryValidateModel(model);

            var campaignModel = Cache_Get_PromotionModel();
            campaignModel.Promotion.Segmentation = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            Cache_Set_PromotionModel(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepDiscount(DiscountStepModel model, string id, string mode, int? ruleIndex = null, string actionType = null)
        {
            // Direct validation to prevent side effects like saving the discount when canceled
            if (model.SelectedRule != null)
            {
                bool isSubsequentCombinedReward = model.SelectedRule.IsSubsequentCombinedDiscount == true;

                bool hasDirectModelStateError = false;
                foreach (string modelStateKey in DiscountRuleModel.GetModelStateValidationKeys(isSubsequentCombinedReward))
                {
                    hasDirectModelStateError = hasDirectModelStateError || HasModelStateErrors($"SelectedRule.{modelStateKey}");
                }

                if (model.SelectedRule.MatchMoreUnits && model.SelectedRule.ApplicableAmount != model.SelectedRule.RequiredAmount)
                {
                    ModelState.AddModelError("SelectedRule.ApplicableAmount", "When match more units is checked, No. of times to apply must be equal to the required amount");
                    hasDirectModelStateError = true;
                }

                if (hasDirectModelStateError)
                {
                    return PartialView(ReturnDiscountRuleModelEditorTemplateName(), model);
                }
            }

            var campaignModel = Cache_Get_PromotionModel();
            if (model.SelectedRule != null)
            {
                var cachedSelectedRule = campaignModel.Promotion.Discount.DiscountRules.FirstOrDefault(r => r.Id == model.SelectedRule.Id) ?? new DiscountRuleModel();
                var modelSelectedRule = model.SelectedRule ?? new DiscountRuleModel();
                modelSelectedRule.SelectedProducts = modelSelectedRule.SelectedProducts ?? new List<ProductModel>();
                modelSelectedRule.TargetProducts = modelSelectedRule.TargetProducts ?? new List<ProductModel>();
                LoadProductsFromMultipleProductUpload(modelSelectedRule.Triggers, cachedSelectedRule.Triggers, "Triggers");
                LoadProductsFromMultipleProductUpload(modelSelectedRule.SelectedProducts, cachedSelectedRule.SelectedProducts, "SelectedProducts");
                LoadProductsFromMultipleProductUpload(modelSelectedRule.TargetProducts, cachedSelectedRule.TargetProducts, "TargetProducts");
            }

            var rule = model.SelectedRule;
            var ruletype = model.RuleType;
            var rules = model.DiscountRules;
            var cachedModel = RetrieveModelFromCache(id) as DiscountStepModel;
            cachedModel.RuleType = ruletype;

            // Copy shared field values when having multiple discounts
            if (cachedModel.DiscountRules.Count >= 2)
            {
                CopySharedFieldsForMultipleDiscounts(cachedModel);
            }

            // Set the removed rule
            if (actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                cachedModel.DiscountRules[ruleIndex.Value].Remove = true;
                ruleIndex = null;
            }

            if (rule != null)
            {
                if (rule.Id == 0)
                {
                    var ruleId = cachedModel.DiscountRules.Count > 0 ? cachedModel.DiscountRules.Min(sr => sr.Id) - 1 : -1;
                    if (ruleId >= 0) ruleId = -1;
                    rule.Id = ruleId;
                    rule.New = true;
                    cachedModel.DiscountRules.Add(rule);
                }
                else
                {
                    var srIndex = cachedModel.DiscountRules.Select((s, index) => new { index, s })
                        .First(sr => sr.s.Id == rule.Id)
                        .index;
                    cachedModel.DiscountRules[srIndex] = rule;
                }
                cachedModel.SelectedRule = null;

                campaignModel.Promotion.Discount = cachedModel;
                Cache_Set_PromotionModel(campaignModel);
            }

            // Update the draft cache
            var campaign = PromotionFactory.UpdateCampaignDiscounts(Cache_Get_Promotion(), cachedModel);
            Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);

            // Recollect model from cache for error messages
            cachedModel = RetrieveModelFromCache(id) as DiscountStepModel;

            // Revalidate the model
            ModelState.Clear();
            TryValidateModel(cachedModel);
            cachedModel.RuleType = ruletype;
            campaignModel.Promotion.Discount = cachedModel;

            // Check for error to invalidate the discount section in the create loyalty page
            // so it's not possible to complete the creation of the loyalty program
            if (cachedModel.DiscountRules.Any(m => m.ErrorMessages.Count > 0))
            {
                ruleIndex = cachedModel.DiscountRules.IndexOf(cachedModel.DiscountRules.FirstOrDefault(r => r.Id == rule.Id));
                ModelState.AddModelError("DiscountRules", "1 or more discount rules are invalid");
            }

            // Collect the correct view
            var viewResult = ReturnCorrectView(id, mode, cachedModel, ruleIndex: ruleIndex);

            // Reset the selected RuleType
            campaignModel.Promotion.Discount.RuleType = 0;

            Cache_Set_PromotionModel(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepPromotion(PromotionStepModel model, string id, string mode)
        {
            var campaign = Cache_Get_Promotion();
            campaign.ImageFileName = model.ImageUrl;
            campaign.ImageEmail = model.ImageEmailEnabled ? model.ImageEmail : model.ImageUrl;

            Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);

            model = RetrieveModelFromCache(id) as PromotionStepModel;
            var campaignModel = Cache_Get_PromotionModel();
            campaignModel.Promotion.Promotion = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            Cache_Set_PromotionModel(campaignModel);

            return viewResult;
        }

        #endregion Step Setters

        /// <summary>
        /// Copy shared field values when having multiple discounts
        /// </summary>
        /// <remarks>
        /// These fields can only be entered in the BackOffice for the first reward rule
        /// and therefore are to be copied to the other reward rules.
        /// </remarks>
        protected void CopySharedFieldsForMultipleDiscounts(DiscountStepModel model)
        {
            DiscountRuleModel firstRuleModel = model.DiscountRules.First();
            foreach (var newRuleModel in model.DiscountRules.Skip(1))
            {
                newRuleModel.RepeatableAmount = firstRuleModel.RepeatableAmount;
            }
        }

        protected static void SetSelectedRule(DiscountStepModel discounts, int? ruleIndex)
        {
            discounts.SelectedRule = ruleIndex.HasValue
                ? discounts.DiscountRules.ElementAt(ruleIndex.Value)
                : new DiscountRuleModel { Type = discounts.RuleType, Id = 0 };
            discounts.SelectedRule.Index = ruleIndex;
        }

        [HttpGet]
        public virtual ActionResult DownloadProducts(int selectedRuleIndex, StepType stepType, ProductsType productsType, string fileNamePrefix)
        {
            var products = new List<ProductModel>();

            var campaignModel = Cache_Get_PromotionModel();
            if (campaignModel != null && stepType == StepType.Discount)
            {
                products = GetDiscountProducts(selectedRuleIndex, productsType, campaignModel);
            }

            return GetDownloadProductsFileResult(products, fileNamePrefix);
        }

        private List<ProductModel> GetDiscountProducts(int selectedRuleIndex, ProductsType productsType, T campaignModel)
        {
            var discountRule = campaignModel.Promotion.Discount.DiscountRules.ElementAtOrDefault(selectedRuleIndex);
            return GetProducts(productsType, discountRule);
        }

        #region Placeholder

        public virtual PartialViewResult GetPlaceholders(int? templateId)
        {
            var serviceModel = Cache_Get_Promotion();
            var model = PlaceholderFactory.GetPlaceholderModels(templateId, serviceModel.PlaceholderValues);

            return PartialView("Placeholders", model);
        }

        #endregion Placeholder
    }
}