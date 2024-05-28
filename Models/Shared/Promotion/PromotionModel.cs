using nl.boxplosive.Service.ServiceModel;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class PromotionModel
    {
        public PromotionModel()
            : this(null)
        {
        }

        public PromotionModel(PromotionCampaign campaign)
        {
            Basics = campaign != null ? new BasicStepModel(campaign) : new BasicStepModel();
            Segmentation = campaign != null ? new SegmentationStepModel(campaign) : new SegmentationStepModel();
            Timeframe = campaign != null ? new PromotionTimeframeStepModel(campaign) : new PromotionTimeframeStepModel();
            Discount = new DiscountStepModel();
            Promotion = new PromotionStepModel();

            if (campaign != null)
            {
                Discount.DiscountRules = campaign.Discounts == null
                    ? new List<DiscountRuleModel>()
                    : campaign.Discounts.Select((discount, index) => new DiscountRuleModel(discount, campaign, index)).ToList();

                Promotion.ImageUrl = campaign.ImageFileName;
                Promotion.ImageEmail = campaign.ImageEmail;
            }
        }

        public int Id { get; set; }

        // 1. Basics
        public BasicStepModel Basics { get; set; }

        // 2. Segmentation
        public SegmentationStepModel Segmentation { get; set; }

        // 3. Timeframe
        public PromotionTimeframeStepModel Timeframe { get; set; }

        // 4. Discount
        public DiscountStepModel Discount { get; set; }

        // 5. Promotion
        public PromotionStepModel Promotion { get; set; }
    }
}