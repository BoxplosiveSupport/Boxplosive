using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using Foolproof;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyEventDefinition
{
    public class LoyaltyEventDefinitionModel : VersionedViewModelBase
    {
        public const string Label_Action = "Action";
        public const string Label_CampaignId = "Promotion";
        public const string Label_CustomerIndication = "Customer indication";
        public const string Label_Details = "Details";
        public const string Label_EndsAt = "Ends at";
        public const string Label_LoyaltyProgramId = "Loyalty program";
        public const string Label_LoyaltyRewardId = "Reward";
        public const string Label_Name = "Name";
        public const string Label_NotificationId = "Notification";
        public const string Label_NumberOfPoints = "Number of points";
        public const string Label_RedemptionPeriod = "Redemption period";
        public const string Label_StartsAt = "Starts at";
        public const string Label_Type = "Event";
        public const string Label_TriggerType = "Trigger event";

        public readonly string Field_CampaignId = nameof(CampaignId);
        public readonly string Field_CustomerIndication = nameof(CustomerIndication);
        public readonly string Field_EndsAtDate = nameof(EndsAtDate);
        public readonly string Field_LoyaltyProgramId = nameof(LoyaltyProgramId);
        public readonly string Field_NotificationId = nameof(NotificationId);
        public readonly string Field_NumberOfPoints = nameof(NumberOfPoints);
        public readonly string Field_StartsAtDate = nameof(StartsAtDate);
        public readonly string Field_TriggerType = nameof(TriggerType);

        public readonly string ErrorMessage_StartsAt = "Starts at cannot be in the past";
        public readonly string ErrorMessage_EndsAt = "Ends at must be later than Starts at";

        public LoyaltyEventDefinitionModel()
        {
        }

        public LoyaltyEventDefinitionModel(boxplosive.Business.Sdk.Entities.LoyaltyEventDefinition loyaltyEventDefinition)
        {
            if (loyaltyEventDefinition != null)
            {
                // Details page
                Id = loyaltyEventDefinition.Id;
                Version = loyaltyEventDefinition.Version;
                Name = loyaltyEventDefinition.Name;
                CreatedAt = loyaltyEventDefinition.CreatedAt;
                UpdatedAt = loyaltyEventDefinition.UpdatedAt;
                Type = loyaltyEventDefinition.Type;
                Types = LoyaltyEventDefinitionFactory.EventTypesToSelectList(Type.Value);
                CampaignId = loyaltyEventDefinition.CampaignId;
                Campaigns = LoyaltyEventDefinitionFactory.CampaignsToSelectList(CampaignId, out IList<Campaign> campaigns);
                StartsAt = loyaltyEventDefinition.StartsAt.ToLocalTimeInOperatingTimeZone();
                EndsAt = loyaltyEventDefinition.EndsAt.ToLocalTimeInOperatingTimeZone();
                RedemptionPeriod = loyaltyEventDefinition.RedemptionPeriod;
                Action = loyaltyEventDefinition.Action;
                Actions = LoyaltyEventDefinitionFactory.EventActionsToSelectList(Action.Value);
                NotificationId = loyaltyEventDefinition.NotificationDefinitionId;
                Notifications = LoyaltyEventDefinitionFactory.NotificationsToSelectList(NotificationId,
                    out IList<IDtoNotificationDefinition> notificationDefinitions);
                LoyaltyRewardId = loyaltyEventDefinition.LoyaltyPointsRewardId;
                LoyaltyRewards = LoyaltyEventDefinitionFactory.LoyaltyRewardsToSelectList(LoyaltyRewardId,
                    out IList<LoyaltyPointsReward> loyaltyRewards);
                LoyaltyProgramId = loyaltyEventDefinition.LoyaltyPointsProgramId;
                LoyaltyPrograms = LoyaltyEventDefinitionFactory.LoyaltyProgramsToSelectList(LoyaltyProgramId,
                    out IList<boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram> loyaltyPrograms);
                NumberOfPoints = loyaltyEventDefinition.NumberOfPoints;
                TriggerType = loyaltyEventDefinition.TriggerType;
                TriggerTypes = LoyaltyEventDefinitionFactory.EventTypesToSelectList(TriggerType, isTrigger: true);
                CustomerIndication = loyaltyEventDefinition.CustomerIndication;
                CustomerIndications = LoyaltyEventDefinitionFactory.CustomerIndicationsToSelectList(CustomerIndication);

                Campaign campaign = campaigns.FirstOrDefault(c => c.Id == CampaignId);
                LoyaltyPointsReward loyaltyReward = loyaltyRewards.FirstOrDefault(lr => lr.Id == LoyaltyRewardId);
                boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram loyaltyProgram = loyaltyPrograms.FirstOrDefault(lp => lp.Id == LoyaltyProgramId);
                IDtoNotificationDefinition notification = notificationDefinitions.FirstOrDefault(nd => nd.Id == NotificationId);

                // List page
                TypeDisplayText = LoyaltyEventDefinitionFactory.EventTypeToDisplayText(Type.Value);
                ActionDisplayText = LoyaltyEventDefinitionFactory.EventActionToDisplayText(Action.Value);
                DetailsDisplayText = LoyaltyEventDefinitionFactory.EventToDetailsDisplayText(loyaltyEventDefinition, campaign, notification,
                    loyaltyProgram, loyaltyReward);
            }
        }

        [Required]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        [DisplayName(Label_Type)]
        public LoyaltyEventType? Type { get; set; }

        public List<SelectListItem> Types { get; set; } = LoyaltyEventDefinitionFactory.EventTypesToSelectList();

        /// <summary>
        /// CreateCoupon action.
        /// </summary>
        [Required]
        [DisplayName(Label_CampaignId)]
        public string CampaignId { get; set; }

        public List<SelectListItem> Campaigns { get; set; } = LoyaltyEventDefinitionFactory.CampaignsToSelectList();

        [Required]
        [DisplayName(Label_StartsAt)]
        [DisplayFormat(DataFormatString = DateFormatString)]
        public DateTime? StartsAtDate { get; set; }

        [Required]
        [DisplayName(Label_StartsAt)]
        [DisplayFormat(DataFormatString = TimeFormatString)]
        public TimeSpan? StartsAtTime { get; set; }

        [DisplayName(Label_StartsAt)]
        [DisplayFormat(DataFormatString = DateTimeFormatString)]
        public DateTime? StartsAt
        {
            get { return StartsAtDate?.Add(StartsAtTime ?? new TimeSpan()); }
            private set
            {
                StartsAtDate = value.Value.Date;
                StartsAtTime = value.Value.TimeOfDay;
            }
        }

        [Required]
        [DisplayName(Label_EndsAt)]
        [DisplayFormat(DataFormatString = DateFormatString)]
        public DateTime? EndsAtDate { get; set; }

        [Required]
        [DisplayName(Label_EndsAt)]
        [DisplayFormat(DataFormatString = TimeFormatString)]
        public TimeSpan? EndsAtTime { get; set; }

        [DisplayName(Label_EndsAt)]
        [DisplayFormat(DataFormatString = DateTimeFormatString)]
        public DateTime? EndsAt
        {
            get { return EndsAtDate?.Add(EndsAtTime ?? new TimeSpan()); }
            private set
            {
                EndsAtDate = value.Value.Date;
                EndsAtTime = value.Value.TimeOfDay;
            }
        }

        /// <summary>
        /// CreateCoupon action.
        /// </summary>
        [DisplayName(Label_RedemptionPeriod)]
        [RegularExpression(TimeSpanRegEx, ErrorMessage = TimeSpanRegExMessage)]
        public string RedemptionPeriod { get; set; }

        [Required]
        [DisplayName(Label_Action)]
        public LoyaltyEventAction? Action { get; set; }

        public List<SelectListItem> Actions { get; set; } = LoyaltyEventDefinitionFactory.EventActionsToSelectList();

        /// <summary>
        /// CreateCouponWithTrigger action.
        /// </summary>
        [Required]
        [DisplayName(Label_TriggerType)]
        public LoyaltyEventType? TriggerType { get; set; }

        public List<SelectListItem> TriggerTypes { get; set; } = LoyaltyEventDefinitionFactory.EventTypesToSelectList(isTrigger: true);

        /// <summary>
        /// CreateCouponWithTrigger action.
        /// </summary>
        [Required]
        [DisplayName(Label_CustomerIndication)]
        public CustomerIndication? CustomerIndication { get; set; }

        public List<SelectListItem> CustomerIndications { get; set; } = LoyaltyEventDefinitionFactory.CustomerIndicationsToSelectList();

        /// <summary>
        /// SendNotification action.
        /// </summary>
        [Required]
        [DisplayName(Label_NotificationId)]
        public int? NotificationId { get; set; }

        public List<SelectListItem> Notifications { get; set; } = LoyaltyEventDefinitionFactory.NotificationsToSelectList();

        /// <summary>
        /// NewReward type.
        /// </summary>
        [DisplayName(Label_LoyaltyRewardId)]
        public int? LoyaltyRewardId { get; set; }

        public List<SelectListItem> LoyaltyRewards { get; set; } = LoyaltyEventDefinitionFactory.LoyaltyRewardsToSelectList();

        /// <summary>
        /// GrantPoints action.
        /// </summary>
        [Required]
        [DisplayName(Label_LoyaltyProgramId)]
        public int? LoyaltyProgramId { get; set; }

        public List<SelectListItem> LoyaltyPrograms { get; set; } = LoyaltyEventDefinitionFactory.LoyaltyProgramsToSelectList();

        /// <summary>
        /// GrantPoints action.
        /// </summary>
        [Required]
        [DisplayName(Label_NumberOfPoints)]
        public int? NumberOfPoints { get; set; }

        public string TypeDisplayText { get; set; }
        public string ActionDisplayText { get; set; }
        public string DetailsDisplayText { get; set; }

        public string TypePartialViewName
        {
            get { return GetTypePartialViewName(Type); }
        }

        public static string GetTypePartialViewName(LoyaltyEventType? value)
        {
            switch (value)
            {
                case LoyaltyEventType.NewReward:
                    return MVC.LoyaltyEventDefinition.Views.TypeNewReward;

                case LoyaltyEventType.RewardAlmostExpired:
                    return MVC.LoyaltyEventDefinition.Views.TypeRewardAlmostExpired;

                case LoyaltyEventType.SavingGoalUnlocked:
                    return MVC.LoyaltyEventDefinition.Views.TypeSavingGoalUnlocked;

                default:
                    return MVC.LoyaltyEventDefinition.Views.Empty;
            }
        }

        public string ActionPartialViewName
        {
            get { return GetActionPartialViewName(Action); }
        }

        public static string GetActionPartialViewName(LoyaltyEventAction? value)
        {
            switch (value)
            {
                case LoyaltyEventAction.CreateCoupon:
                    return MVC.LoyaltyEventDefinition.Views.ActionCreateCoupon;

                case LoyaltyEventAction.CreateCouponWithTrigger:
                    return MVC.LoyaltyEventDefinition.Views.ActionCreateCouponWithTrigger;

                case LoyaltyEventAction.GrantPoints:
                    return MVC.LoyaltyEventDefinition.Views.ActionGrantPoints;

                case LoyaltyEventAction.SendNotification:
                    return MVC.LoyaltyEventDefinition.Views.ActionSendNotification;

                default:
                    return MVC.LoyaltyEventDefinition.Views.Empty;
            }
        }

        public bool IsValid(ModelStateDictionary modelState)
        {
            if (Action.HasValue)
            {
                if (Action.Value != LoyaltyEventAction.CreateCoupon && Action.Value != LoyaltyEventAction.CreateCouponWithTrigger)
                {
                    modelState.Remove(Field_CampaignId);
                }

                if (Action.Value != LoyaltyEventAction.CreateCouponWithTrigger)
                {
                    modelState.Remove(Field_CustomerIndication);
                    modelState.Remove(Field_TriggerType);
                }

                if (Action.Value != LoyaltyEventAction.GrantPoints)
                {
                    modelState.Remove(Field_LoyaltyProgramId);
                    modelState.Remove(Field_NumberOfPoints);
                }

                if (Action.Value != LoyaltyEventAction.SendNotification)
                {
                    modelState.Remove(Field_NotificationId);
                }
            }

            if (!StartsAt.HasValue || StartsAt == DateTime.MinValue)
            {
                modelState.AddModelError(Field_StartsAtDate, ErrorMessage_StartsAt);
            }

            if (!EndsAt.HasValue || EndsAt == DateTime.MinValue || EndsAt <= StartsAt)
            {
                modelState.AddModelError(Field_EndsAtDate, ErrorMessage_EndsAt);
            }

            return modelState.IsValid;
        }
    }
}