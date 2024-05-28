using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public enum SavingsRuleTypeModel
    {
        [Display(Name = "Points for every X amount in transaction")]
        LoyaltyPointsOnTransactionByValue = 1,

        [Display(Name = "Points for product value")]
        LoyaltyPointsOnProductsByValue,

        [Display(Name = "Points for product value in transaction")]
        LoyaltyPointsOnProductsInTransactionByValue,

        [Display(Name = "Points for products")]
        FixedLoyaltyPointsOnProducts,

        [Display(Name = "Points for products in transaction")]
        FixedLoyaltyPointsOnProductsInTransaction,

        [Display(Name = "Points for transaction")]
        FixedLoyaltyPointsOnTransaction,

        [Display(Name = "Points for transaction in days of the week")]
        FixedLoyaltyPointsOnTransactionInDaysOfWeek,

        [Display(Name = "Points for unique products in transaction")]
        FixedLoyaltyPointsOnUniqueProductsInTransaction,

        [Display(Name = "QR saving")]
        CodeActionSaving,
    }
}