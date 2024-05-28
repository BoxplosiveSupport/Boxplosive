using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;

namespace nl.boxplosive.BackOffice.Mvc.Models.ModifyPromotion
{
    public class StatusListModel : StepModelBase
    {
        public string PromotionCode { get; set; }

        public bool? Basics { get; set; }
        public bool? Segmentation { get; set; }
        public bool? Timeframe { get; set; }
        public bool? Discount { get; set; }
        public bool? Promotion { get; set; }

        public void SetStatus(PromotionModel promotionModel)
        {
            Basics = promotionModel.Basics.IsValid;
            Segmentation = promotionModel.Segmentation.IsValid;
            Timeframe = promotionModel.Timeframe.IsValid;
            Discount = promotionModel.Discount.IsValid;
            Promotion = promotionModel.Promotion.IsValid;
        }

        public string BasicsStyle
        {
            get
            {
                if (Basics.HasValue && Basics.Value)
                {
                    return "text-success";
                }

                if (Basics.HasValue && !Basics.Value)
                {
                    return "text-danger";
                }

                return "";
            }
        }

        public string SegmentationStyle
        {
            get
            {
                if (Segmentation.HasValue && Segmentation.Value)
                {
                    return "text-success";
                }

                if (Segmentation.HasValue && !Segmentation.Value)
                {
                    return "text-danger";
                }

                return "";
            }
        }

        public string TimeframeStyle
        {
            get
            {
                if (Timeframe.HasValue && Timeframe.Value)
                {
                    return "text-success";
                }

                if (Timeframe.HasValue && !Timeframe.Value)
                {
                    return "text-danger";
                }

                return "";
            }
        }

        public string DiscountStyle
        {
            get
            {
                if (Discount.HasValue && Discount.Value)
                {
                    return "text-success";
                }

                if (Discount.HasValue && !Discount.Value)
                {
                    return "text-danger";
                }

                return "";
            }
        }

        public string PromotionStyle
        {
            get
            {
                if (Promotion.HasValue && Promotion.Value)
                {
                    return "text-success";
                }

                if (Promotion.HasValue && !Promotion.Value)
                {
                    return "text-danger";
                }

                return "";
            }
        }

        public string PublishStyle
        {
            get
            {
                if (Basics.HasValue && Basics.Value &&
                       Timeframe.HasValue && Timeframe.Value &&
                       Discount.HasValue && Discount.Value &&
                       Promotion.HasValue && Promotion.Value)
                {
                    return "";
                }

                return "disabled";
            }
        }
    }
}