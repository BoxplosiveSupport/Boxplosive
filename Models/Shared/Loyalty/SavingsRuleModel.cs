using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Foolproof;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Service.ServiceModel.Campaign;
using nl.boxplosive.Utilities.Extensions;
using nl.boxplosive.Utilities.Json;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class SavingsRuleModel
    {
        private const string ErrorMessageNoProducts = "No products selected";
        private const string ErrorMessageApplicableGreaterThanRequired = "No. of times to apply can't be greater than Minimum no. of products";

        public static readonly IList<string> ModelStateValidationKeys = new List<string>() { };

        public SavingsRuleModel()
            : this(null)
        {
        }

        public SavingsRuleModel(Saving saving)
        {
            ErrorMessages = new List<string>();
            SelectedProducts = new List<ProductModel>();

            RequiredAmount = 1;
            ApplicableAmount = 1;
            RepeatableAmount = int.MaxValue;

            if (saving != null)
            {
                CouponCode = saving.CouponCode;
                Id = saving.Discount.Id;
                Name = saving.Name;
                RepeatableAmount = saving.RepeatableAmount;
                Type = (SavingsRuleTypeModel)Enum.Parse(typeof(SavingsRuleTypeModel), saving.Discount.Type, true);
                Amount = saving.Discount.Amount;
                RequiredAmount = saving.Discount.RequiredAmount;
                ApplicableAmount = saving.Discount.ApplicableAmount;
                MinTransactionValue = saving.Discount.MinTransactionValue;
                SuppressPointsForRewards = saving.Discount.SuppressPointsForRewards;

                StartDate = saving.StartsAt.ToLocalTimeInOperatingTimeZone().Date;
                EndDate = saving.EndsAt.ToLocalTimeInOperatingTimeZone().Date;
                From = saving.StartsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay;
                To = saving.EndsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay;

                if (Type == SavingsRuleTypeModel.FixedLoyaltyPointsOnTransactionInDaysOfWeek)
                {
                    DaysOfWeek = (DayRuleChecks)saving.Discount.DaysOfWeek.Value;

                    Monday = (DayRuleChecks.Monday & DaysOfWeek) != 0;
                    Tuesday = (DayRuleChecks.Tuesday & DaysOfWeek) != 0;
                    Wednesday = (DayRuleChecks.Wednesday & DaysOfWeek) != 0;
                    Thursday = (DayRuleChecks.Thursday & DaysOfWeek) != 0;
                    Friday = (DayRuleChecks.Friday & DaysOfWeek) != 0;
                    Saturday = (DayRuleChecks.Saturday & DaysOfWeek) != 0;
                    Sunday = (DayRuleChecks.Sunday & DaysOfWeek) != 0;
                }

                if (Type == SavingsRuleTypeModel.FixedLoyaltyPointsOnProducts
                    || Type == SavingsRuleTypeModel.LoyaltyPointsOnProductsByValue
                    || Type == SavingsRuleTypeModel.LoyaltyPointsOnTransactionByValue)
                {
                    SelectedProducts = saving.Discount.ProductIds == null
                        ? new List<ProductModel>()
                        : CampaignFactory.CreateProductModelList(saving.Discount.ProductIds);

                    if (!ProductsOptional && SelectedProducts.Count == 0)
                    {
                        ErrorMessages.Add(ErrorMessageNoProducts);
                    }

                    if (ApplicableAmount > RequiredAmount)
                    {
                        ErrorMessages.Add(ErrorMessageApplicableGreaterThanRequired);
                    }
                }

                // CodeActionSaving
                if (saving.CodeAction != null)
                {
                    CodeAction codeAction = saving.CodeAction;
                    var additionalSettings = JsonWrapper.DeserializeObject<CodeActionAdditionalSettings>(codeAction.AdditionalSettings);

                    CodeAction_Id = codeAction.Id;
                    CodeAction_Tag = codeAction.Tag;
                    CodeAction_AdditionalSettings_Points = additionalSettings.Points;
                    CodeActionType_Type = (CodeActionTypeModel)codeAction.ActionTypeId;
                }
            }

            SetIsPublished(saving);
        }

        public CodeAction ToCodeActionServiceModel()
        {
            if (Type != SavingsRuleTypeModel.CodeActionSaving)
                return null;

            var additionalSettings = new CodeActionAdditionalSettings()
            {
                Points = CodeAction_AdditionalSettings_Points
            };
            string additionalSettingsJson = JsonWrapper.SerializeObject(additionalSettings);

            return new CodeAction()
            {
                Id = CodeAction_Id,
                TenantId = "tid1",
                CampaignId = CouponCode,
                ActionTypeId = (int)CodeActionType_Type,
                StartsAt = StartDate.Add(From).ToUtcInOperatingTimeZone(),
                EndsAt = EndDate.Add(To).ToUtcInOperatingTimeZone(),
                Tag = CodeAction_Tag,
                AdditionalSettings = additionalSettingsJson,
            };
        }

        public int? Index { get; set; }

        public string CouponCode { get; set; }

        public int Id { get; set; }

        /// <summary>
        /// Name of the saving rule
        /// </summary>
        [Required]
        public string Name { get; set; }

        /// <summary>
        /// New is set to <c>true</c> when the rule is inserted inside MVC.
        /// It resets to <c>false</c> as soon as it's inserted in the cache
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// If set to <c>true</c>, this record will be deleted
        /// </summary>
        public bool Remove { get; set; }

        /// <summary>
        /// If set to <c>true</c>, this record will be disabled
        /// </summary>
        public bool Disable { get; set; }

        /// <summary>
        /// Used to identify errors in the rules
        /// </summary>
        public List<string> ErrorMessages { get; set; }

        /// <summary>
        /// The discount rule type, kind of promotion.
        /// </summary>
        /// <remarks>
        /// This is the DiscountRuleTreeType as string.
        /// </remarks>
        [DisplayName("Saving type")]
        public SavingsRuleTypeModel Type { get; set; }

        /// <summary>
        /// The selected products.
        /// </summary>
        /// <remarks>
        /// This is <c>null</c> when the saving rule has no support for products.
        /// </remarks>
        public List<ProductModel> SelectedProducts { get; set; }

        /// <summary>
        /// the number of selected products.
        /// </summary>
        [DisplayName("Applies to products")]
        public string NumberOfProductsText
        {
            get
            {
                if (SelectedProducts == null)
                {
                    return "";
                }

                int productCount = SelectedProducts.Count;
                return ProductsOptional && productCount == 0 ? "All products" : productCount.ToString();
            }
        }

        /// <summary>
        /// If the products are optional.
        /// </summary>
        /// <remarks>
        /// Products are optional is <c>True</c> otherwise <c>False</c>.
        /// </remarks>
        public bool ProductsOptional => Type == SavingsRuleTypeModel.LoyaltyPointsOnTransactionByValue;

        /// <summary>
        /// The amount.
        /// </summary>
        /// <remarks>
        /// As per ELP-1938 this also allows for negative values and as such allows a range from int.MinValue to int.MaxValue
        /// if it doesn't adhere to these rules, i.e. it is not a valid integer, a new ValidationHelper errormessage will be shown.
        /// </remarks>
        [Range(int.MinValue, int.MaxValue, ErrorMessage = ValidationHelper.ValueMustBeAnInteger)]
        public int Amount { get; set; }

        public string AmountText
        {
            get
            {
                switch (Type)
                {
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnProducts:
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnProductsInTransaction:
                        return "Loyalty pts. per product";

                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnTransaction:
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnTransactionInDaysOfWeek:
                        return "Loyalty pts. for transaction";

                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnUniqueProductsInTransaction:
                        return "Loyalty pts. per product";

                    case SavingsRuleTypeModel.LoyaltyPointsOnProductsByValue:
                    case SavingsRuleTypeModel.LoyaltyPointsOnProductsInTransactionByValue:
                        return "Cents per loyalty point";

                    case SavingsRuleTypeModel.LoyaltyPointsOnTransactionByValue:
                        return "Loyalty pts. for transaction value";

                    default:
                        return "Amount";
                }
            }
        }

        /// <summary>
        /// Whether receiving points should be suppressed when a reward was cleared in the same transaction.
        /// </summary>
        [Display(Name = "Suppress points for rewards")]
        public bool SuppressPointsForRewards { get; set; }

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
            get { return _RequiredAmount; }
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
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnProducts:
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnProductsInTransaction:
                    case SavingsRuleTypeModel.LoyaltyPointsOnProductsByValue:
                        return "Minimum no. of products";

                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnTransaction:
                    case SavingsRuleTypeModel.FixedLoyaltyPointsOnTransactionInDaysOfWeek:
                        return "Minimum transaction value (Cents)";

                    case SavingsRuleTypeModel.LoyaltyPointsOnProductsInTransactionByValue:
                        return "Maximum no. of products";

                    case SavingsRuleTypeModel.LoyaltyPointsOnTransactionByValue:
                        return "Transaction value (cents)";

                    default:
                        return "Minimum no. of products";
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
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        [LessThanOrEqualTo("RequiredAmount", ErrorMessage = "This value must be lower than or equal to the minimum no. of products")]
        public int ApplicableAmount { get; set; }

        public string ApplicableAmountText
        {
            get { return "No. of times to apply"; }
        }

        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int RepeatableAmount { get; set; }

        public string RepeatableAmountText
        {
            get { return "No. of times to repeat"; }
        }

        public int? MinTransactionValue
        {
            get { return _MinTransactionValue; }
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

        private bool _HasRequiredMinTransactionValue
        {
            get
            {
                return Type == SavingsRuleTypeModel.FixedLoyaltyPointsOnTransaction
                    || Type == SavingsRuleTypeModel.FixedLoyaltyPointsOnTransactionInDaysOfWeek
                    || Type == SavingsRuleTypeModel.LoyaltyPointsOnTransactionByValue;
            }
        }

        [DisplayName("Start")]
        public DateTime StartDate { get; set; }

        [DisplayName("End")]
        [GreaterThanOrEqualTo("StartDate", ErrorMessage = "End must be greater than or equal to Start.")]
        public DateTime EndDate { get; set; }

        public TimeSpan From { get; set; }

        public TimeSpan To { get; set; }

        [DisplayName("Days of the week")]
        public DayRuleChecks DaysOfWeek { get; set; }

        public bool Monday { get; set; }
        public bool Tuesday { get; set; }
        public bool Wednesday { get; set; }
        public bool Thursday { get; set; }
        public bool Friday { get; set; }
        public bool Saturday { get; set; }
        public bool Sunday { get; set; }

        public bool StartDateTodayOrInThePast
        {
            get { return StartDate <= DateTime.Today; }
        }

        public bool EndDateInThePast
        {
            get { return EndDate < DateTime.Today; }
        }

        public bool IsPublished { get; set; }

        private void SetIsPublished(Saving saving)
        {
            IsPublished = saving != null && saving.Discount.Id != -1;
        }

        #region CodeActionSaving

        /// <remarks>
        /// Hidden field.
        /// </remarks>
        public int CodeAction_Id { get; set; }

        [DisplayName("QR code action tag")]
        [Required]
        public string CodeAction_Tag { get; set; }

        [DisplayName("Loyalty points for scan")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int CodeAction_AdditionalSettings_Points { get; set; }

        /// <remarks>
        /// This is hard-coded for now because only 'Code_OneTimeUse' is implemented.
        /// </remarks>
        [DisplayName("QR code action type")]
        public CodeActionTypeModel CodeActionType_Type { get; set; } = CodeActionTypeModel.Code_OneTimeUse;

        #endregion CodeActionSaving
    }
}