using Foolproof;
using MessagePack;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Campaign;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class RewardsRuleModel : ViewModelBase, IProductListsModel
    {
        private const string ErrorMessageRewardsNoProducts = "No products selected";

        public bool AppCardStackDefinitionsEnabled { get; } = AppConfig.Settings.Feature_FlexibleAppCardStacks;

        public RewardsRuleModel()
            : base()
        {
        }

        public RewardsRuleModel(RewardBase reward, bool? combinedReward = null)
            : this()
        {
            RepeatableAmount = combinedReward.HasValue && combinedReward.Value ? 1 : int.MaxValue;

            MaxNumberOfRedemptions = 1;
            RequiredNumberOfCoupons = 1;

            if (reward != null)
            {
                // Base settings
                Id = reward.Id;
                Name = reward.Name;
                RewardAppCardStackDefinitionId = reward.RewardAppCardStackDefinitionId;
                RewardAppCardOrder = reward.RewardAppCardOrder;
                AppCardTemplateId = reward.AppCardTemplateId;
                ExternalRewardSupplierId = reward.ExternalRewardSupplierId;
                StockStatus = reward.StockStatus.ConvertToModel();
                LoyaltyPointTreshold = reward.Threshold;
                LoyaltyPointDelta = reward.Delta;
                ImageUrl = reward.ImageFileName;
                Description = reward.Description;
                StartsAtDate = reward.StartsAt.ToLocalTimeInOperatingTimeZone().Date;
                StartsAtTime = reward.StartsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay;
                EndsAtDate = reward.EndsAt.ToLocalTimeInOperatingTimeZone().Date;
                EndsAtTime = reward.EndsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay;
                RepeatableAmount = reward.RepeatableAmount;
                RedemptionLimitDate = reward.RedemptionLimit?.ToLocalTimeInOperatingTimeZone().Date;
                RedemptionLimitTime = reward.RedemptionLimit?.ToLocalTimeInOperatingTimeZone().TimeOfDay;
                RedemptionPeriod = reward.RedemptionPeriod.ToCanonicalString();
                MaxNumberOfRedemptions = reward.NumberOfRedemptions;
                MailThreshold = reward.MailThreshold;
                CampaignExternalId = reward.CampaignExternalId;
                Placeholders = PlaceholderFactory.GetPlaceholderModels(AppCardTemplateId, reward.PlaceholderValues);

                if (reward.SavingGoalAppCardStackDefinitionId.HasValue)
                    SavingGoalAppCardStackDefinitionId = reward.SavingGoalAppCardStackDefinitionId.Value;
                SavingGoalAppCardOrder = reward.SavingGoalAppCardOrder;

                switch (RewardType)
                {
                    case LoyaltyPointsRewardType.Claim:
                        var claimReward = (ClaimReward)reward;

                        Type = (DiscountRuleType)Enum.Parse(typeof(DiscountRuleType), ((ClaimReward)reward).Discount.Type, true);
                        PayOutLoyaltyProgramId = claimReward.Discount.PayOutLoyaltyProgramId;
                        RequiredNumberOfCoupons = claimReward.Discount.RequiredNumberOfCoupons;
                        MaxNumberOfCouponsPerTransaction = claimReward.Discount.MaxNumberOfCouponsPerTransaction;
                        Amount = claimReward.Discount.Amount;
                        TargetAmount = claimReward.Discount.TargetAmount;
                        RequiredAmount = claimReward.Discount.RequiredAmount;
                        ApplicableAmount = claimReward.Discount.ApplicableAmount;
                        MinTransactionValue = claimReward.Discount.MinTransactionValue;
                        MatchMoreUnits = claimReward.Discount.MatchMoreUnits;
                        Message = claimReward.Discount.Message;
                        MessageType = (MessageType)Enum.Parse(typeof(MessageType), claimReward.Discount.MessageType, true);
                        GrantPointsLoyaltyProgramId = claimReward.Discount.LoyaltyProgramId;
                        NumberOfLoyaltyPoints = claimReward.Discount.Amount;

                        if (claimReward.Discount.Triggers != null && claimReward.Discount.Triggers.Count > 0)
                        {
                            Triggers = CampaignFactory.CreateProductModelList(claimReward.Discount.Triggers);
                        }

                        SelectedProducts = new List<ProductModel>();
                        if (HasSelectedProducts)
                        {
                            if (claimReward.Discount.ProductIds != null && claimReward.Discount.ProductIds.Count > 0)
                            {
                                SelectedProducts = CampaignFactory.CreateProductModelList(claimReward.Discount.ProductIds);
                            }
                            else
                            {
                                ErrorMessages.Add(ErrorMessageRewardsNoProducts);
                            }
                        }

                        if (HasTargetProducts)
                        {
                            if (claimReward.Discount.TargetProductIds != null && claimReward.Discount.TargetProductIds.Count > 0)
                            {
                                TargetProducts = CampaignFactory.CreateProductModelList(claimReward.Discount.TargetProductIds);
                            }
                            else
                            {
                                ErrorMessages.Add(ErrorMessageRewardsNoProducts);
                            }
                        }

                        break;
                }
            }
        }

        public void CopySharedFieldsForCombinedRewards(RewardsRuleModel other)
        {
            Id = other.Id;
            Name = other.Name;
            RewardAppCardStackDefinitionId = other.RewardAppCardStackDefinitionId;
            AppCardTemplateId = other.AppCardTemplateId;
            ExternalRewardSupplierId = other.ExternalRewardSupplierId;
            StockStatus = other.StockStatus;
            LoyaltyPointTreshold = other.LoyaltyPointTreshold;
            LoyaltyPointDelta = other.LoyaltyPointDelta;
            ImageUrl = other.ImageUrl;
            Description = other.Description;
            StartsAtDate = other.StartsAtDate;
            StartsAtTime = other.StartsAtTime;
            EndsAtDate = other.EndsAtDate;
            EndsAtTime = other.EndsAtTime;
            RepeatableAmount = other.RepeatableAmount;
            RedemptionLimitDate = other.RedemptionLimitDate;
            RedemptionLimitTime = other.RedemptionLimitTime;
            RedemptionPeriod = other.RedemptionPeriod;
            MaxNumberOfRedemptions = other.MaxNumberOfRedemptions;
            MailThreshold = other.MailThreshold;
            CampaignExternalId = other.CampaignExternalId;
            Placeholders = other.Placeholders;
            SavingGoalAppCardStackDefinitionId = other.SavingGoalAppCardStackDefinitionId;
        }

        public int? Index { get; set; }

        public int Id { get; set; }

        public bool? IsSubsequentCombinedReward { get; set; }

        [Required]
        [StringLength(50, ErrorMessage = "Name has a maximum of 50 characters")]
        public string Name { get; set; }

        [DisplayName("Campaign external ID")]
        [Required]
        [StringLength(100, ErrorMessage = "Campaign external ID has a maximum of 100 characters")]
        public string CampaignExternalId { get; set; }

        public List<SelectListItem> AppCardStackDefinitions => AppCardStackDefinitionFactory.AppCardStackDefinitionToSelectList();

        [Display(Name = "Saving goal AppCard stack")]
        [RequiredIfTrue(nameof(AppCardStackDefinitionsEnabled), ErrorMessage = "The Saving goal AppCard stack field is required.")]
        public int SavingGoalAppCardStackDefinitionId { get; set; }

        [DisplayName("Saving goal AppCard order")]
        [Required]
        public int? SavingGoalAppCardOrder { get; set; } = 100;

        [Display(Name = "Reward AppCard stack")]
        [RequiredIfTrue(nameof(AppCardStackDefinitionsEnabled), ErrorMessage = "The Reward AppCard stack field is required.")]
        public int RewardAppCardStackDefinitionId { get; set; }

        [DisplayName("Reward AppCard order")]
        [Required]
        public int RewardAppCardOrder { get; set; } = 100;

        #region AppCardTemplateId

        [DisplayName("AppCard template")]
        [RequiredIfTrue(nameof(AppCardTemplateIdIsDisplayed), ErrorMessage = "The AppCard template field is required.")]
        public int? AppCardTemplateId { get; set; }

        public List<SelectListItem> AppCardTemplateId_SelectListItems => TemplateFactory.CreateSelectList(TemplateType.Reward, AppCardTemplateId);

        public string AppCardTemplateId_Name
        {
            get
            {
                SelectListItem selectedItem = AppCardTemplateId_SelectListItems
                    .FirstOrDefault(item => item.Value == AppCardTemplateId?.ToString());

                return selectedItem?.Text ?? AppCardTemplateId_SelectListItems.FirstOrDefault()?.Text;
            }
        }

        public bool AppCardTemplateIdIsDisplayed => AppConfig.Settings.Feature_AppCardTemplate;

        #endregion AppCardTemplateId

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

        /// <summary>
        /// New is set to <c>true</c> when the rule is inserted inside MVC.
        /// It resets to <c>false</c> as soon as it's inserted in the cache
        /// </summary>
        public bool New { get; set; }

        /// <summary>
        /// If set to <c>true</c>, this record will be deleted
        /// </summary>
        /// <remarks>
        /// This only works in the draft flow, not for publicated rewards
        /// </remarks>
        public bool Remove { get; set; }

        /// <summary>
        /// If set to <c>true</c>, this record will be disabled
        /// </summary>
        public bool Disable { get; set; }

        ///// <summary>
        ///// Used to identify errors in the rules
        ///// </summary>
        public List<string> ErrorMessages { get; set; } = new List<string>();

        /// <summary>
        /// The discount rule type, kind of promotion.
        /// </summary>
        /// <remarks>
        /// This is the DiscountRuleTreeType as string.
        /// </remarks>
        [DisplayName("Reward type")]
        public DiscountRuleType Type { get; set; }

        /// <summary>
        /// The discount triggers.
        /// </summary>
        public List<ProductModel> Triggers { get; set; } = new List<ProductModel>();

        /// <summary>
        /// The selected products
        /// </summary>
        public List<ProductModel> SelectedProducts { get; set; } = new List<ProductModel>();

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
        public List<ProductModel> TargetProducts { get; set; } = new List<ProductModel>();

        public bool HasTargetProducts => Type == DiscountRuleType.BuyXGetYFree;

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

        private int _RequiredAmount = 1;

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
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        [LessThanOrEqualTo("RequiredAmount", ErrorMessage = "This value must be lower than or equal to the required no. of products")]
        public int ApplicableAmount { get; set; } = 1;

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

        private int _repeatableAmount = int.MaxValue;

        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int RepeatableAmount
        {
            get => _repeatableAmount;
            set { _repeatableAmount = HasRepeatableAmount ? value : 1; }
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

        [DisplayName("Match more units")]
        public bool MatchMoreUnits { get; set; }

        public bool HasMatchMoreUnits => Type == DiscountRuleType.CentsDiscountOnProducts
            || Type == DiscountRuleType.PercentsDiscountOnProducts
            || Type == DiscountRuleType.FixedPriceOnProducts;

        [DisplayName("Message")]
        [Required]
        [DataType(DataType.MultilineText)]
        public string Message { get; set; }

        [DisplayName("Message type")]
        public MessageType MessageType { get; set; } = MessageType.Receipt;

        [Required]
        [DisplayName("Required no. of points")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int LoyaltyPointTreshold { get; set; }

        [DisplayName("Loyalty pts.")]
        public int LoyaltyPointDelta { get; set; }

        [DisplayName("Image")]
        [IgnoreMember]
        [RequiredIfEmpty("ImageUrl", ErrorMessage = "An image is required")]
        public HttpPostedFileBase Image { get; set; }

        public string ImageUrl
        {
            get => _ImageUrl;
            set => _ImageUrl = SdkConfig.Current.OverrideImageUrl(value);
        }

        private string _ImageUrl;

        [Required]
        [AllowHtml]
        [ToStringLength(2500, ErrorMessage = "Description has a maximum of 2500 characters")]
        public HtmlEditor Description { get; set; }

        public LoyaltyPointsRewardType RewardType => LoyaltyPointsRewardType.Claim;

        [DisplayName("Start")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime StartsAtDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan StartsAtTime { get; set; }

        [DisplayName("End")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [GreaterThanOrEqualTo("StartsAtDate", ErrorMessage = "End must be greater than or equal to Start.")]
        public DateTime EndsAtDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}")]
        public TimeSpan EndsAtTime { get; set; }

        [DisplayName("Redemption limit")]
        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        [GreaterThanOrEqualTo("StartsAtDate", ErrorMessage = "Redemption limit must be greater than or equal to Start.", PassOnNull = true)]
        public DateTime? RedemptionLimitDate { get; set; }

        [DisplayFormat(DataFormatString = "{0:hh\\:mm}")]
        [RequiredIfNotEmpty("RedemptionLimitDate", ErrorMessage = "The Redemption limit is required.")]
        public TimeSpan? RedemptionLimitTime { get; set; }

        [DisplayName("Redemption period (days)")]
        [RegularExpression(TimeSpanRegEx, ErrorMessage = TimeSpanRegExMessage)]
        public string RedemptionPeriod { get; set; }

        [DisplayName("Max number of redemptions")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int MaxNumberOfRedemptions { get; set; }

        [DisplayName("Mail threshold")]
        public int? MailThreshold { get; set; }

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

        public IList<PlaceholderModel> Placeholders { get; set; } = new List<PlaceholderModel>();

        public void Validate(bool validateProducts, bool validateRequiredProducts)
        {
            if (!New
                && ((validateProducts && (SelectedProducts == null || SelectedProducts.Count == 0))
                    || (validateRequiredProducts && (TargetProducts == null || TargetProducts.Count == 0))
            ))
            {
                ErrorMessages.Add(ErrorMessageRewardsNoProducts);
            }
        }

        public bool StartDateTodayOrInThePast => StartsAtDate <= DateTime.Today;

        public bool EndDateInThePast => EndsAtDate < DateTime.Today;

        public bool IsPublished => Id > 0;

        public string SavingGoalAppCardStackDefinitionDisplayName => AppCardStackDefinitionFactory.GetAppCardStackDefinitionDisplayName(SavingGoalAppCardStackDefinitionId);

        public string RewardAppCardStackDefinitionDisplayName => AppCardStackDefinitionFactory.GetAppCardStackDefinitionDisplayName(RewardAppCardStackDefinitionId);

        public static IList<string> GetModelStateValidationKeys(bool isSubsequentCombinedReward)
        {
            var result = new List<string>();
            if (!isSubsequentCombinedReward)
            {
                result.Add(nameof(AppCardTemplateId));
                result.Add(nameof(ExternalRewardSupplierId));
            }

            return result;
        }

        public bool EndsAfterLoyaltyCampaign(LoyaltyCampaign campaign) =>
            this.EndsAtDate.Add(this.EndsAtTime) >= campaign.EndDate.ToLocalTimeInOperatingTimeZone();
    }
}