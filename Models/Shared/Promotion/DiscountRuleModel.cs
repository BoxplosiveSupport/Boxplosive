using Foolproof;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class DiscountRuleModel : StepModelBase, IProductListsModel
    {
        public const string ErrorMessageSavingsNoProducts = "No products selected";

        public DiscountRuleModel()
            : this(discountRule: null, campaign: null, indexOfDiscountRuleInCollection: 0)
        {
        }

        public DiscountRuleModel(DiscountRule discountRule, PromotionCampaign campaign, int indexOfDiscountRuleInCollection)
        {
            ErrorMessages = new List<string>();
            Triggers = new List<ProductModel>();
            SelectedProducts = new List<ProductModel>();
            TargetProducts = new List<ProductModel>();

            RequiredAmount = 1;
            ApplicableAmount = 1;
            RepeatableAmount = int.MaxValue;
            MaxNumberOfRedemptions = 1;
            RequiredNumberOfCoupons = 1;

            MessageType = MessageType.Receipt;

            if (discountRule != null)
            {
                // ELP-6320: Make sure that all Id values are set correctly, otherwise update behavior is broken
                //   model.Id == 0 --> is equal to new rule
                //      (index * -1) = switch index to a negative number
                //      Example: 1st discount --> Id = -1, 2nd --> Id = -2, 3rd --> Id = -3 ...
                Id = discountRule.Id;
                if (Id == 0)
                    Id = (indexOfDiscountRuleInCollection * -1) - 1;

                DiscountRuleType type;
                Enum.TryParse(discountRule.Type, true, out type);

                Type = type;
                PayOutLoyaltyProgramId = discountRule.PayOutLoyaltyProgramId;
                RequiredNumberOfCoupons = discountRule.RequiredNumberOfCoupons;
                MaxNumberOfCouponsPerTransaction = discountRule.MaxNumberOfCouponsPerTransaction;
                Amount = discountRule.Amount;
                TargetAmount = discountRule.TargetAmount;
                RequiredAmount = discountRule.RequiredAmount;
                ApplicableAmount = discountRule.ApplicableAmount;
                MinTransactionValue = discountRule.MinTransactionValue;
                MatchMoreUnits = discountRule.MatchMoreUnits;
                Message = discountRule.Message;
                MessageType = (MessageType)Enum.Parse(typeof(MessageType), discountRule.MessageType, true);
                GrantPointsLoyaltyProgramId = discountRule.LoyaltyProgramId;
                NumberOfLoyaltyPoints = discountRule.Amount;

                if (discountRule.Triggers != null && discountRule.Triggers.Count > 0)
                {
                    Triggers = CampaignFactory.CreateProductModelList(discountRule.Triggers);
                }

                if (HasSelectedProducts)
                {
                    if (discountRule.ProductIds != null && discountRule.ProductIds.Count > 0)
                    {
                        SelectedProducts = CampaignFactory.CreateProductModelList(discountRule.ProductIds);
                    }
                    else
                    {
                        ErrorMessages.Add(ErrorMessageSavingsNoProducts);
                    }
                }

                if (HasTargetProducts)
                {
                    if (discountRule.TargetProductIds != null && discountRule.TargetProductIds.Count > 0)
                    {
                        TargetProducts = CampaignFactory.CreateProductModelList(discountRule.TargetProductIds);
                    }
                    else
                    {
                        ErrorMessages.Add(ErrorMessageSavingsNoProducts);
                    }
                }
            }

            if (campaign != null)
            {
                EndDateInThePast = campaign.EndDate < DateTime.Today;
                ExternalRewardSupplierId = campaign.ExternalRewardSupplierId;
                MaxNumberOfRedemptions = campaign.NumberOfRedemptions;
                RepeatableAmount = campaign.RepeatableAmount;
                StockStatus = campaign.StockStatus.ConvertToModel();
            }
        }

        public int? Index { get; set; }

        public int Id { get; set; }

        public bool? IsSubsequentCombinedDiscount { get; set; }

        /// <summary>
        /// <c>True</c> if a new discount rule is just added, otherwise <c>false</c>.
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// If set to <c>true</c>, this record will be deleted
        /// </summary>
        public bool Remove { get; set; }

        /// <summary>
        /// The discount rule type, kind of promotion.
        /// </summary>
        /// <remarks>
        /// This is the DiscountRuleTreeType as string.
        /// </remarks>
        [DisplayName("Discount type")]
        public DiscountRuleType Type { get; set; }

        /// <summary>
        /// Used to identify errors in the rules
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// The discount triggers.
        /// </summary>
        public List<ProductModel> Triggers { get; set; }

        /// <summary>
        /// The selected products
        /// </summary>
        public List<ProductModel> SelectedProducts { get; set; }

        public bool HasTriggerProducts => Type != DiscountRuleType.ExternalReward && Type != DiscountRuleType.VisualRedemption;

        public bool HasSelectedProducts => Type != DiscountRuleType.Message
            && Type != DiscountRuleType.CentsDiscountOnTransaction
            && Type != DiscountRuleType.CentsDiscountAtYProducts
            && Type != DiscountRuleType.PercentsDiscountOnTransaction
            && Type != DiscountRuleType.PercentsDiscountAtYProducts
            && Type != DiscountRuleType.GrantLoyaltyPoints
            && Type != DiscountRuleType.ExternalReward
            && Type != DiscountRuleType.VisualRedemption;

        /// <summary>
        /// The required products
        /// </summary>
        public List<ProductModel> TargetProducts { get; set; }

        public bool HasTargetProducts => Type == DiscountRuleType.BuyXGetYFree;

        [DisplayName("Max number of redemptions")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int MaxNumberOfRedemptions { get; set; }

        /// <summary>
        /// The amount.
        /// </summary>
        /// <remarks>
        /// Purpose differs per type:
        /// - PercentsDiscountOnProducts = percents,
        /// - FixedPriceOnProducts = price (in cents),
        /// - XPlusYFree = Y, the free number of products
        /// </remarks>
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int Amount { get; set; }

        public string AmountText
        {
            get
            {
                switch (Type)
                {
                    case DiscountRuleType.PercentsDiscountOnProducts:
                        return "Discount (Percentage)";

                    case DiscountRuleType.CentsDiscountOnProducts:
                    case DiscountRuleType.CentsDiscountOnProductSet:
                        return "Discount (Cents)";

                    case DiscountRuleType.FixedPriceOnProductSet:
                    case DiscountRuleType.FixedPriceOnProducts:
                        return "Price (Cents)";

                    case DiscountRuleType.CentsDiscountOnTransaction:
                    case DiscountRuleType.CentsDiscountAtYProducts:
                        return "Transaction discount (Cents)";

                    case DiscountRuleType.PercentsDiscountAtYProducts:
                    case DiscountRuleType.PercentsDiscountOnTransaction:
                        return "Transaction discount (Percentage)";

                    default:
                        return "Amount";
                }
            }
        }

        /// <summary>
        /// The 'target' amount.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int TargetAmount { get; set; }

        public string TargetAmountText => "Amount";

        /// <summary>
        /// The required amount.
        /// </summary>
        /// <remarks>
        /// Purpose differs per type:
        /// - PercentsDiscountOnProducts = required number of products,
        /// - FixedPriceOnProducts = required number of products,
        /// - XPlusYFree = X, the required number (excludes the free one)
        /// </remarks>
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int RequiredAmount
        {
            get => _RequiredAmount;
            set
            {
                // This value acts like MinTransactionValue for several rule types
                if (_HasRequiredMinTransactionValue)
                {
                    _MinTransactionValue = value;
                }

                _RequiredAmount = value;
            }
        }

        private int _RequiredAmount;

        public string RequiredAmountText
        {
            get
            {
                switch (Type)
                {
                    case DiscountRuleType.CentsDiscountOnTransaction:
                    case DiscountRuleType.PercentsDiscountOnTransaction:
                        return "Minimum transaction value (Cents)";

                    case DiscountRuleType.XPlusYFree:
                    case DiscountRuleType.BuyXGetYFree:
                        return "No. of products (X)";

                    case DiscountRuleType.FixedPriceOnProductSet:
                        return "No. of products";

                    case DiscountRuleType.CentsDiscountOnProductSet:
                        return "Minimum value of matched units (Cents)";

                    default:
                        return "Required Amount";
                }
            }
        }

        /// <summary>
        /// The amount.
        /// </summary>
        /// <remarks>
        /// Purpose differs per type:
        /// - PercentsDiscountOnProducts = percents,
        /// - FixedPriceOnProducts = price (in cents)
        /// </remarks>
        [LessThanOrEqualTo("RequiredAmount", ErrorMessage = "This value must be lower then or equal to the required no. of products")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int ApplicableAmount { get; set; }

        public string ApplicableAmountText
        {
            get
            {
                switch (Type)
                {
                    case DiscountRuleType.XPlusYFree:
                    case DiscountRuleType.BuyXGetYFree:
                        return "No. of products (Y)";

                    default:
                        return "No. of times to apply";
                }
            }
        }

        private int _repeatableAmount;

        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int RepeatableAmount
        {
            get => _repeatableAmount;
            set => _repeatableAmount = HasRepeatableAmount ? value : 1;
        }

        public string RepeatableAmountText => "No. of times to repeat";

        public bool HasRepeatableAmount => Type != DiscountRuleType.Message
            && Type != DiscountRuleType.CentsDiscountOnTransaction
            && Type != DiscountRuleType.CentsDiscountAtYProducts
            && Type != DiscountRuleType.PercentsDiscountOnTransaction
            && Type != DiscountRuleType.PercentsDiscountAtYProducts
            && Type != DiscountRuleType.GrantLoyaltyPoints
            && Type != DiscountRuleType.ExternalReward
            && Type != DiscountRuleType.VisualRedemption;

        [DisplayName("Required number of coupons")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        [Required]
        public int? RequiredNumberOfCoupons { get; set; }

        #region Field - MaxNumberOfCouponsPerTransaction

        [DisplayName("Max no. of coupons per transaction")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int? MaxNumberOfCouponsPerTransaction { get; set; }

        public bool MaxNumberOfCouponsPerTransaction_IsDisplayed => Type == DiscountRuleType.CentsDiscountAtYProducts
            || Type == DiscountRuleType.CentsDiscountOnTransaction
            || Type == DiscountRuleType.PercentsDiscountAtYProducts
            || Type == DiscountRuleType.PercentsDiscountOnTransaction;

        public string MaxNumberOfCouponsPerTransaction_DisplayText => MaxNumberOfCouponsPerTransaction.HasValue ? MaxNumberOfCouponsPerTransaction.Value.ToString() : "Unlimited";

        #endregion Field - MaxNumberOfCouponsPerTransaction

        [DisplayName("Minimum transaction value (Cents)")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int? MinTransactionValue
        {
            get => _MinTransactionValue;
            set
            {
                // The RequiredAmount field acts like required MinTransactionValue for several rule types
                if (_HasRequiredMinTransactionValue)
                {
                    _RequiredAmount = value.HasValue ? value.Value : 0;
                }

                _MinTransactionValue = value;
            }
        }

        private int? _MinTransactionValue;

        [DisplayName("Minimum transaction value (Cents)")]
        public string MinTransactionValueDisplayText => MinTransactionValue.HasValue ? MinTransactionValue.Value.ToString() : "No minimum";

        public bool MinTransactionValue_IsDisplayed => Type != DiscountRuleType.CentsDiscountOnTransaction
            && Type != DiscountRuleType.PercentsDiscountOnTransaction
            && Type != DiscountRuleType.ExternalReward
            && Type != DiscountRuleType.VisualRedemption;

        private bool _HasRequiredMinTransactionValue => !MinTransactionValue_IsDisplayed;

        public bool HasMatchMoreUnits => Type == DiscountRuleType.CentsDiscountOnProducts
            || Type == DiscountRuleType.PercentsDiscountOnProducts
            || Type == DiscountRuleType.FixedPriceOnProducts;

        [DisplayName("Match more units")]
        public bool MatchMoreUnits { get; set; }

        [DataType(DataType.MultilineText)]
        [DisplayName("Message")]
        [Required]
        public string Message { get; set; }

        [DisplayName("Message type")]
        public MessageType MessageType { get; set; }

        [DisplayName("Number of loyalty points")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int NumberOfLoyaltyPoints { get; set; }

        #region LoyaltyPointsProgramId

        [DisplayName("Loyalty program")]
        [Required]
        public int? GrantPointsLoyaltyProgramId { get; set; }

        public List<SelectListItem> GrantPointsLoyaltyProgramId_SelectListItems =>
            LoyaltyFactory.GetLoyaltyPrograms(GrantPointsLoyaltyProgramId, "- Select a program -");

        public string GrantPointsLoyaltyProgramId_Name
        {
            get
            {
                SelectListItem selectedItem = GrantPointsLoyaltyProgramId_SelectListItems
                    .FirstOrDefault(l => l.Value == GrantPointsLoyaltyProgramId?.ToString());

                return selectedItem?.Text ?? GrantPointsLoyaltyProgramId_SelectListItems.FirstOrDefault()?.Text;
            }
        }

        #endregion LoyaltyPointsProgramId

        #region PayOutLoyaltyProgramId

        [DisplayName("Discount pay out")]
        public int? PayOutLoyaltyProgramId { get; set; }

        public List<SelectListItem> PayOutLoyaltyProgramId_SelectListItems => LoyaltyFactory.GetLoyaltyPrograms(PayOutLoyaltyProgramId, "In transaction");

        public string PayOutLoyaltyProgramId_Name
        {
            get
            {
                SelectListItem selectedItem = PayOutLoyaltyProgramId_SelectListItems
                    .FirstOrDefault(l => l.Value == PayOutLoyaltyProgramId?.ToString());

                return selectedItem?.Text ?? PayOutLoyaltyProgramId_SelectListItems.FirstOrDefault()?.Text;
            }
        }

        public bool PayOutLoyaltyProgramId_IsDisplayed => AppConfig.Settings.Feature_DiscountPayOutToLoyaltyProgram
            && Type != DiscountRuleType.Message
            && Type != DiscountRuleType.GrantLoyaltyPoints
            && Type != DiscountRuleType.ExternalReward
            && Type != DiscountRuleType.VisualRedemption;

        #endregion PayOutLoyaltyProgramId

        #region ExternalRewardSupplierId

        public bool HasRewardSupplier => Type == DiscountRuleType.ExternalReward;

        [DisplayName("Reward supplier")]
        [RequiredIfTrue(nameof(HasRewardSupplier), ErrorMessage = "The Reward supplier field is required.")]
        public int? ExternalRewardSupplierId { get; set; }

        public List<SelectListItem> ExternalRewardSupplierId_SelectListItems => ExternalRewardSupplierFactory.CreateSelectList(ExternalRewardSupplierId);

        public string ExternalRewardSupplierId_Name
        {
            get
            {
                SelectListItem selectedItem = ExternalRewardSupplierId_SelectListItems
                    .FirstOrDefault(item => item.Value == ExternalRewardSupplierId?.ToString());

                return selectedItem?.Text ?? ExternalRewardSupplierId_SelectListItems.FirstOrDefault()?.Text;
            }
        }

        #endregion ExternalRewardSupplierId

        #region StockStatus

        [DisplayName("Stock status")]
        public StockStatusModel StockStatus { get; set; }

        #endregion StockStatus

        public void Validate(bool validateProducts, bool validateRequiredProducts)
        {
            if (!New
                && ((validateProducts && (SelectedProducts == null || SelectedProducts.Count == 0))
                    || (validateRequiredProducts && (TargetProducts == null || TargetProducts.Count == 0))
            ))
            {
                ErrorMessages.Add(ErrorMessageSavingsNoProducts);
            }
        }

        public bool EndDateInThePast { get; private set; }

        public bool IsPublished { get; set; }

        private void SetIsPublished(DiscountRule discountRule)
        {
            IsPublished = discountRule != null && discountRule.Id != -1;
        }

        public static IList<string> GetModelStateValidationKeys(bool isSubsequentCombinedDiscount)
        {
            var result = new List<string>();
            if (!isSubsequentCombinedDiscount)
            {
                result.Add(nameof(ExternalRewardSupplierId));
            }

            return result;
        }
    }
}