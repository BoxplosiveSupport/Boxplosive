using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
    public enum DiscountRuleType
    {
        [Display(Name = "% Discount")]
        PercentsDiscountOnProducts = 1,

        [Display(Name = "% Transaction discount (product count)")]
        PercentsDiscountAtYProducts,

        [Display(Name = "% Transaction discount (value)")]
        PercentsDiscountOnTransaction,

        [Display(Name = "Buy X get Y free")]
        BuyXGetYFree,

        [Display(Name = "Cents discount")]
        CentsDiscountOnProducts,

        [Display(Name = "Cents discount on product set")]
        CentsDiscountOnProductSet,

        [Display(Name = "Cents transaction discount (product count)")]
        CentsDiscountAtYProducts,

        [Display(Name = "Cents transaction discount (value)")]
        CentsDiscountOnTransaction,

        [Display(Name = "External reward")]
        ExternalReward,

        [Display(Name = "Fixed price")]
        FixedPriceOnProducts,

        [Display(Name = "Fixed price for product set")]
        FixedPriceOnProductSet,

        [Display(Name = "Grant points")]
        GrantLoyaltyPoints,

        [Display(Name = "Message")]
        Message,

        [Display(Name = "Visual redemption")]
        VisualRedemption,

        [Display(Name = "X + Y free")]
        XPlusYFree,
    }

    public enum MessageType
    {
        [Display(Name = "Employee Display")]
        EmployeeDisplay = 1,

        [Display(Name = "Customer Display")]
        CustomerDisplay = 2,

        [Display(Name = "Receipt")]
        Receipt = 4,

        [Display(Name = "None")]
        None = 8,
    }

    public enum CampaignType
    {
        [Display(Name = "Show all types")]
        All = 0,

        [Display(Name = "Show promotions")]
        Promotion = 1,

        [Display(Name = "Show loyalty programs")]
        Loyalty = 2,
    }

    public enum FieldType
    {
        Asset,
        Bool,
        HTML,
        Int,
        String,
        Text,
        JSON
    }
}