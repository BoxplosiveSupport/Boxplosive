using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Mapping;
using nl.boxplosive.BackOffice.Mvc.Models.CreatePromotion;
using nl.boxplosive.BackOffice.Mvc.Models.ModifyPromotion;
using nl.boxplosive.BackOffice.Mvc.Models.PublishPromotion;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using CreatePromotion = nl.boxplosive.BackOffice.Mvc.Models.CreatePromotion;
using ModifyPromotion = nl.boxplosive.BackOffice.Mvc.Models.ModifyPromotion;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class PromotionFactory : BaseFactory
    {
        public static CreatePromotionModel CreateCampaignModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrel,
            string campaignDescription,
            PromotionCampaign campaign)
        {
            var model = new CreatePromotionModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                StatusList = new CreatePromotion.StatusListModel(),
                Promotion = new PromotionModel(campaign)
            };

            return model;
        }

        public static ModifyPromotionModel ModifyCampaignModel(string pageTitle,
           string pageNavigationLabel,
           string pageNavigationUrel,
           string campaignDescription,
           PromotionCampaign campaign)
        {
            var model = new ModifyPromotionModel
            {
                PageTitle = String.Format("{0} - {1}", pageTitle, campaign.Id),
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                StatusList = new ModifyPromotion.StatusListModel
                {
                    PromotionCode = campaign.Id
                },
                PromotionCode = campaign.Id,
                Promotion = new PromotionModel(campaign)
            };

            return model;
        }

        public static PublishPromotionModel CreatePublishCampaignModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrel,
            string campaignDescription,
            PromotionCampaign campaign)
        {
            var model = new PublishPromotionModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrel,
                PageNavigationUrlText = pageNavigationLabel,
                Promotion = new PromotionModel(campaign)
            };

            return model;
        }

        public static CampaignProcessRequest ConvertCampaignToPublishRequest(PromotionCampaign campaign, CampaignProcess process)
        {
            var request = new CampaignProcessRequest();
            AuthenticationHelpers.SetupServiceRequest(request, "Publish");
            campaign.MaximumCouponCount = int.MaxValue;
            campaign.StartDate = campaign.StartDate.ToUtcInOperatingTimeZone();
            campaign.EndDate = campaign.EndDate.ToUtcInOperatingTimeZone();

            request.Process = process.Map();
            request.Process.Type = CampaignProcessType.Publish;
            request.Process.Campaign = campaign;

            return request;
        }

        public static PromotionCampaign UpdateCampaignDiscounts(PromotionCampaign campaign, DiscountStepModel model,
            List<string> selectedUploadProducts = null, List<string> targetUploadProducts = null, List<string> triggerUploadProducts = null)
        {
            if (campaign.Discounts == null)
            {
                campaign.Discounts = new List<DiscountRule>();
            }

            int discountIndex = 0;
            int combinedDiscountIndex = 0;
            foreach (var discount in model.DiscountRules)
            {
                if (discount.Remove)
                {
                    campaign.Discounts.RemoveAt(model.DiscountRules.IndexOf(discount));
                }
                else
                {
                    DiscountRule serviceDiscount;
                    // Check if the saving is just added
                    if (!discount.New) // update the existing saving
                    {
                        serviceDiscount = campaign.Discounts.ElementAt(combinedDiscountIndex);
                        combinedDiscountIndex++;
                    }
                    else // Insert a new saving
                    {
                        serviceDiscount = new DiscountRule
                        {
                            Id = discount.Id,
                            Type = discount.Type.ToString()
                        };
                        campaign.Discounts.Add(serviceDiscount);
                    }

                    serviceDiscount.Triggers = ProductFactory.ProcessProducts(discount.Triggers, triggerUploadProducts);
                    serviceDiscount.ProductIds = ProductFactory.ProcessProducts(discount.SelectedProducts, selectedUploadProducts);
                    serviceDiscount.TargetProductIds = ProductFactory.ProcessProducts(discount.TargetProducts, targetUploadProducts);

                    serviceDiscount.Type = discount.Type.ToString();
                    serviceDiscount.PayOutLoyaltyProgramId = discount.PayOutLoyaltyProgramId;
                    serviceDiscount.RequiredNumberOfCoupons = discount.RequiredNumberOfCoupons;
                    serviceDiscount.MaxNumberOfCouponsPerTransaction = discount.MaxNumberOfCouponsPerTransaction;
                    serviceDiscount.Amount = discount.Amount;
                    serviceDiscount.TargetAmount = discount.TargetAmount;
                    serviceDiscount.RequiredAmount = discount.RequiredAmount;
                    serviceDiscount.ApplicableAmount = discount.ApplicableAmount;
                    serviceDiscount.Message = discount.Message;
                    serviceDiscount.MessageType = discount.MessageType.ToString();
                    serviceDiscount.MinTransactionValue = discount.MinTransactionValue;
                    serviceDiscount.MatchMoreUnits = discount.MatchMoreUnits;

                    if (discount.Type == DiscountRuleType.GrantLoyaltyPoints)
                    {
                        serviceDiscount.LoyaltyProgramId = discount.GrantPointsLoyaltyProgramId;
                        serviceDiscount.Amount = discount.NumberOfLoyaltyPoints;
                    }

                    // Shared fields are defined by the first discount rule
                    if (discountIndex == 0)
                    {
                        campaign.ExternalRewardSupplierId = discount.ExternalRewardSupplierId;
                        campaign.NumberOfRedemptions = discount.MaxNumberOfRedemptions;
                        campaign.RepeatableAmount = discount.RepeatableAmount;
                        campaign.StockStatus = discount.StockStatus.ConvertFromModel();
                    }

                    discountIndex++;
                }
            }

            return campaign;
        }
    }
}