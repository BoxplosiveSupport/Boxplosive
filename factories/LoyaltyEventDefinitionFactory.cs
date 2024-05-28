using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.LoyaltyEventDefinition;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Utilities.Extensions;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class LoyaltyEventDefinitionFactory : BaseFactory
    {
        public const string PageTitle = "Loyalty event";
        private const string _OverviewPageTitle = "Loyalty events";

        private const string _TypeUnknownText = "Choose event type";
        private const string _TypeUnknownTriggerText = "Choose trigger event type";
        private const string _TypeLoyaltyCardScannedText = "Loyalty card scanned";
        private const string _TypeNewCustomerText = "New customer";
        private const string _TypeNewInviteeText = "New invitee";
        private const string _TypeNewRewardText = "New reward";
        private const string _TypePointsEarnedAtPosText = "Points earned at the POS";
        private const string _TypeRewardAlmostExpiredText = "Reward almost expired";
        private const string _TypeRewardInviterText = "Reward inviter";
        private const string _TypeSavingGoalUnlockedText = "Saving goal unlocked";
        private const string _TypeShareLoyaltyPointsAcceptedText = "Share loyalty points accepted";

        private const string _ActionUnknownText = "Choose event action";
        private const string _ActionCreateCouponText = "Create coupon";
        private const string _ActionCreateCouponWithTriggerText = "Create coupon with trigger";
        private const string _ActionGrantPointsText = "Grant points";
        private const string _ActionSendNotificationText = "Send notification";

        private const string _CustomerIndicationDetailsTextFormat = "customer indication: {CustomerIndication}";
        private const string _LoyaltyProgramDetailsTextFormat = "loyalty program: {LoyaltyProgramName}";
        private const string _LoyaltyRewardDetailsTextFormat = "loyalty reward: {LoyaltyRewardName}";
        private const string _NotificationDetailsTextFormat = "notification: {CustomEvent}";
        private const string _NumberOfPointsDetailsTextFormat = "number of points: {NumberOfPoints}";
        private const string _PromotionDetailsTextFormat = "promotion: {PromotionTitle}";
        private const string _RedemptionDetailsTextFormat = "redemption: {RedemptionPeriod}";
        private const string _TriggerTypeDetailsTextFormat = "trigger event: {TriggerType}";

        private const string _CustomerIndicationUnknownText = "Choose customer indication";

        private static ICampaignApi _CampaignApi => BusinessApiFactory.GetInstance().BusinessApi<ICampaignApi>();
        private static ILoyaltyPointsApi _LoyaltyPointsApi => BusinessApiFactory.GetInstance().BusinessApi<ILoyaltyPointsApi>();

        private static ILoyaltyPointsRewardRepository _LoyaltyPointsRewardRepository =>
            DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyPointsRewardRepository>();

        private static INotificationDefinitionRepository _NotificationDefinitionRepository =>
            DataRepositoryFactory.GetInstance().DataRepository<INotificationDefinitionRepository>();

        /// <summary>
        /// Returns a single LoyaltyEventDefinitionModel
        /// </summary>
        public LoyaltyEventDefinitionModel CreateLoyaltyEventDefinitionModel(LoyaltyEventDefinition loyaltyEventDefinition, UrlHelper urlHelper,
            int? page = null)
        {
            var result = new LoyaltyEventDefinitionModel(loyaltyEventDefinition);
            SetPageNavigationValuesForDetailPage(result, urlHelper, page);

            return result;
        }

        /// <summary>
        /// Returns a list of LoyaltyEventDefinitionModels
        /// </summary>
        public LoyaltyEventDefinitionsModel CreateLoyaltyEventDefinitionsModel(LoyaltyEventDefinitions loyaltyEventDefinitions, UrlHelper urlHelper,
            int? page = null)
        {
            var result = new LoyaltyEventDefinitionsModel(loyaltyEventDefinitions);
            result.SetPageFields(page, _OverviewPageTitle, urlHelper.Action(MVC.Home.ActionNames.Index, MVC.Home.Name), PageConst.Title_NavigateToHomePage);

            return result;
        }

        public LoyaltyEventDefinition ModelToLoyaltyEventDefinition(LoyaltyEventDefinitionModel model)
        {
            var dto = new DtoLoyaltyEventDefinition
            {
                Id = model.Id,
                Version = model.Version,
                Name = model.Name.Trim(),
                CreatedAt = model.CreatedAt != DateTime.MinValue ? model.CreatedAt : DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Type = model.Type.Value,
                CampaignId = model.CampaignId,
                StartsAt = model.StartsAt.Value.ToUtcInOperatingTimeZone(),
                EndsAt = model.EndsAt.Value.ToUtcInOperatingTimeZone(),
                RedemptionPeriod = model.RedemptionPeriod,
                Action = model.Action.Value,
                NotificationDefinitionId = model.NotificationId,
                LoyaltyPointsRewardId = model.LoyaltyRewardId,
                LoyaltyPointsProgramId = model.LoyaltyProgramId,
                NumberOfPoints = model.NumberOfPoints,
                TriggerType = model.TriggerType,
                CustomerIndication = model.CustomerIndication
            };

            return new LoyaltyEventDefinition(dto);
        }

        public static string EventToDetailsDisplayText(
            LoyaltyEventDefinition loyaltyEventDefinition,
            Campaign campaign = null,
            IDtoNotificationDefinition notificationDefinition = null,
            LoyaltyPointsProgram loyaltyProgram = null,
            LoyaltyPointsReward loyaltyReward = null)
        {
            var details = new List<string>();

            // NewReward type
            if (loyaltyEventDefinition.LoyaltyPointsRewardId.HasValue)
            {
                loyaltyReward = loyaltyReward ?? new LoyaltyPointsReward(_LoyaltyPointsRewardRepository.Get(loyaltyEventDefinition.LoyaltyPointsRewardId.Value));
                details.Add(Smart.Format(_LoyaltyRewardDetailsTextFormat, new { LoyaltyRewardName = loyaltyReward?.Name }));
            }

            // CreateCoupon action
            if (loyaltyEventDefinition.CampaignId != null)
            {
                campaign = campaign ?? _CampaignApi.GetCampaign(loyaltyEventDefinition.CampaignId);
                details.Add(Smart.Format(_PromotionDetailsTextFormat, new { PromotionTitle = campaign?.Title }));
            }

            if (loyaltyEventDefinition.RedemptionPeriod != null)
            {
                details.Add(Smart.Format(_RedemptionDetailsTextFormat, new { RedemptionPeriod = loyaltyEventDefinition.RedemptionPeriod }));
            }

            // CreateCouponWithTrigger action
            if (loyaltyEventDefinition.TriggerType.HasValue)
            {
                details.Add(Smart.Format(_TriggerTypeDetailsTextFormat, new { TriggerType = loyaltyEventDefinition.TriggerType }));
            }

            if (loyaltyEventDefinition.CustomerIndication.HasValue)
            {
                details.Add(Smart.Format(_CustomerIndicationDetailsTextFormat, new { CustomerIndication = loyaltyEventDefinition.CustomerIndication }));
            }

            // GrantPoints action
            if (loyaltyEventDefinition.LoyaltyPointsProgramId.HasValue)
            {
                loyaltyProgram = loyaltyProgram ?? _LoyaltyPointsApi.GetProgram(loyaltyEventDefinition.LoyaltyPointsProgramId.Value);
                details.Add(Smart.Format(_LoyaltyProgramDetailsTextFormat, new { LoyaltyProgramName = loyaltyProgram?.Name }));
            }

            if (loyaltyEventDefinition.NumberOfPoints.HasValue)
            {
                details.Add(Smart.Format(_NumberOfPointsDetailsTextFormat, new { NumberOfPoints = loyaltyEventDefinition.NumberOfPoints }));
            }

            // SendNotification action
            if (loyaltyEventDefinition.NotificationDefinitionId.HasValue)
            {
                notificationDefinition = notificationDefinition ?? _NotificationDefinitionRepository.Get(loyaltyEventDefinition.NotificationDefinitionId.Value);
                details.Add(Smart.Format(_NotificationDetailsTextFormat, new { CustomEvent = notificationDefinition?.CustomEvent }));
            }

            return string.Join(", ", details);
        }

        public static List<SelectListItem> EventTypesToSelectList(LoyaltyEventType? selected = null, bool isTrigger = false)
        {
            List<SelectListItem> loyaltyEventSelectList = new List<SelectListItem>();
            loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.Unknown, selected, isTrigger, isDefaultOption: true));
            if (!isTrigger)
            {
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.LoyaltyCardScanned, selected, isTrigger));
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.NewCustomer, selected, isTrigger));
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.NewReward, selected, isTrigger));
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.PointsEarnedAtPos, selected, isTrigger));
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.RewardAlmostExpired, selected, isTrigger));
                loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.SavingGoalUnlocked, selected, isTrigger));
            }

            loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.NewInvitee, selected, isTrigger));
            loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.RewardInviter, selected, isTrigger));
            loyaltyEventSelectList.Add(CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType.SharePointsAccepted, selected, isTrigger));

            return loyaltyEventSelectList.OrderBy(les => les.Value).ToList();
        }

        public static string EventTypeToDisplayText(LoyaltyEventType value, bool isTrigger = false)
        {
            switch (value)
            {
                case LoyaltyEventType.Unknown:
                    return isTrigger ? _TypeUnknownTriggerText : _TypeUnknownText;

                case LoyaltyEventType.LoyaltyCardScanned:
                    return _TypeLoyaltyCardScannedText;

                case LoyaltyEventType.NewCustomer:
                    return _TypeNewCustomerText;

                case LoyaltyEventType.NewInvitee:
                    return _TypeNewInviteeText;

                case LoyaltyEventType.NewReward:
                    return _TypeNewRewardText;

                case LoyaltyEventType.PointsEarnedAtPos:
                    return _TypePointsEarnedAtPosText;

                case LoyaltyEventType.RewardAlmostExpired:
                    return _TypeRewardAlmostExpiredText;

                case LoyaltyEventType.RewardInviter:
                    return _TypeRewardInviterText;

                case LoyaltyEventType.SavingGoalUnlocked:
                    return _TypeSavingGoalUnlockedText;

                case LoyaltyEventType.SharePointsAccepted:
                    return _TypeShareLoyaltyPointsAcceptedText;

                default:
                    throw new NotImplementedException($"Event type: {value}");
            }
        }

        private static SelectListItem CreateLoyaltyEventTypeSelectListItem(LoyaltyEventType value, LoyaltyEventType? selected, bool isTrigger,
            bool isDefaultOption = false)
        {
            return new SelectListItem
            {
                Value = isDefaultOption ? string.Empty : value.ToString(),
                Text = EventTypeToDisplayText(value, isTrigger),
                Selected = selected == value
            };
        }

        public static List<SelectListItem> EventActionsToSelectList(LoyaltyEventAction selected = LoyaltyEventAction.Unknown)
        {
            return new List<SelectListItem>()
            {
                CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction.Unknown, selected, isDefaultOption: true),
                CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction.CreateCoupon, selected),
                CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction.CreateCouponWithTrigger, selected),
                CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction.GrantPoints, selected),
                CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction.SendNotification, selected),
            };
        }

        public static string EventActionToDisplayText(LoyaltyEventAction value)
        {
            switch (value)
            {
                case LoyaltyEventAction.Unknown:
                    return _ActionUnknownText;

                case LoyaltyEventAction.CreateCoupon:
                    return _ActionCreateCouponText;

                case LoyaltyEventAction.CreateCouponWithTrigger:
                    return _ActionCreateCouponWithTriggerText;

                case LoyaltyEventAction.GrantPoints:
                    return _ActionGrantPointsText;

                case LoyaltyEventAction.SendNotification:
                    return _ActionSendNotificationText;

                default:
                    throw new NotImplementedException($"Event action: {value}");
            }
        }

        private static SelectListItem CreateLoyaltyEventActionSelectListItem(LoyaltyEventAction type, LoyaltyEventAction selected,
            bool isDefaultOption = false)
        {
            return new SelectListItem
            {
                Value = isDefaultOption ? string.Empty : type.ToString(),
                Text = EventActionToDisplayText(type),
                Selected = selected == type
            };
        }

        public static List<SelectListItem> CampaignsToSelectList()
        {
            return CampaignsToSelectList(selected: null, campaigns: out IList<Campaign> campaigns);
        }

        public static List<SelectListItem> CampaignsToSelectList(string selected, out IList<Campaign> campaigns)
        {
            var itemList = new List<SelectListItem>();
            campaigns = new List<Campaign>();

            bool hasSelection = (selected != null);
            itemList.Add(new SelectListItem { Value = string.Empty, Text = "Choose a campaign", Selected = !hasSelection });

            Campaign selectedCampaign = null;
            if (hasSelection)
            {
                selectedCampaign = CampaignApi.GetCampaign(selected);
                itemList.Add(CreateCampaignSelectListItem(selectedCampaign, selected));

                campaigns.Add(selectedCampaign);
            }

            IList<Campaign> unexpiredCampaigns = CampaignApi.GetUnexpiredCampaigns();
            foreach (Campaign campaign in unexpiredCampaigns)
            {
                // Skip selected
                if (selectedCampaign != null && campaign.Id == selectedCampaign.Id)
                {
                    continue;
                }

                itemList.Add(CreateCampaignSelectListItem(campaign, selected));

                campaigns.Add(campaign);
            }

            return itemList;
        }

        public static List<SelectListItem> CustomerIndicationsToSelectList(CustomerIndication? selected = null)
        {
            return new List<SelectListItem>()
            {
                CreateCustomerIndicationSelectListItem(CustomerIndication.Unknown, selected, isDefaultOption: true),
                CreateCustomerIndicationSelectListItem(CustomerIndication.Primary, selected),
                CreateCustomerIndicationSelectListItem(CustomerIndication.Secondary, selected)
            };
        }

        public static string CustomerIndicationToDisplayText(CustomerIndication value)
        {
            switch (value)
            {
                case CustomerIndication.Unknown:
                    return _CustomerIndicationUnknownText;

                case CustomerIndication.Primary:
                    return nameof(CustomerIndication.Primary);

                case CustomerIndication.Secondary:
                    return nameof(CustomerIndication.Secondary);

                default:
                    throw new NotImplementedException($"Customer indication: {value}");
            }
        }

        private static SelectListItem CreateCustomerIndicationSelectListItem(CustomerIndication value, CustomerIndication? selected, bool isDefaultOption = false)
        {
            return new SelectListItem
            {
                Value = isDefaultOption ? string.Empty : value.ToString(),
                Text = CustomerIndicationToDisplayText(value),
                Selected = selected == value
            };
        }

        public static List<SelectListItem> LoyaltyProgramsToSelectList()
        {
            return LoyaltyProgramsToSelectList(selected: null, loyaltyPrograms: out IList<LoyaltyPointsProgram> loyaltyPrograms);
        }

        public static List<SelectListItem> LoyaltyProgramsToSelectList(int? selected, out IList<LoyaltyPointsProgram> loyaltyPrograms)
        {
            var itemList = new List<SelectListItem>();
            loyaltyPrograms = new List<LoyaltyPointsProgram>();

            itemList.Add(new SelectListItem { Value = string.Empty, Text = "Choose a loyalty program", Selected = !selected.HasValue });

            LoyaltyPointsProgram selectedLoyaltyProgram = null;
            if (selected.HasValue)
            {
                selectedLoyaltyProgram = _LoyaltyPointsApi.GetProgram(selected.Value);
                itemList.Add(CreateLoyaltyProgramSelectListItem(selectedLoyaltyProgram, selected));

                loyaltyPrograms.Add(selectedLoyaltyProgram);
            }

            IList<LoyaltyPointsProgram> unexpiredLoyaltyPrograms = LoyaltyPointsApi.GetAllProgramsByStatuses(
                new HashSet<int>(), new HashSet<int>(), false,
                LoyaltyPointsProgramStatus.Inactive, LoyaltyPointsProgramStatus.Active, LoyaltyPointsProgramStatus.TemporaryInactive);
            foreach (LoyaltyPointsProgram loyaltyProgram in unexpiredLoyaltyPrograms)
            {
                // Skip selected
                if (selectedLoyaltyProgram != null && loyaltyProgram.Id == selectedLoyaltyProgram.Id)
                {
                    continue;
                }

                itemList.Add(CreateLoyaltyProgramSelectListItem(loyaltyProgram, selected));

                loyaltyPrograms.Add(loyaltyProgram);
            }

            return itemList;
        }

        public static List<SelectListItem> LoyaltyRewardsToSelectList()
        {
            return LoyaltyRewardsToSelectList(selected: null, loyaltyRewards: out IList<LoyaltyPointsReward> loyaltyRewards);
        }

        public static List<SelectListItem> LoyaltyRewardsToSelectList(int? selected, out IList<LoyaltyPointsReward> loyaltyRewards)
        {
            var itemList = new List<SelectListItem>();
            loyaltyRewards = new List<LoyaltyPointsReward>();

            itemList.Add(new SelectListItem { Value = string.Empty, Text = "All rewards", Selected = !selected.HasValue });

            LoyaltyPointsReward selectedLoyaltyReward = null;
            if (selected.HasValue)
            {
                selectedLoyaltyReward = new LoyaltyPointsReward(_LoyaltyPointsRewardRepository.Get(selected.Value));
                LoyaltyPointsProgram selectedLoyaltyProgram = new LoyaltyPointsProgram(selectedLoyaltyReward.Campaign.DtoCampaign.LoyaltyPointsProgram);
                itemList.Add(CreateLoyaltyRewardSelectListItem(selectedLoyaltyReward, selectedLoyaltyProgram, selected));

                loyaltyRewards.Add(selectedLoyaltyReward);
            }

            IList<LoyaltyPointsProgram> unexpiredLoyaltyPrograms = LoyaltyPointsApi.GetAllProgramsByStatuses(
                new HashSet<int>(), new HashSet<int>(), false,
                LoyaltyPointsProgramStatus.Inactive, LoyaltyPointsProgramStatus.Active, LoyaltyPointsProgramStatus.TemporaryInactive);
            foreach (LoyaltyPointsProgram loyaltyProgram in unexpiredLoyaltyPrograms)
            {
                foreach (LoyaltyPointsReward loyaltyReward in LoyaltyPointsApi.GetProgram(loyaltyProgram.Id).Rewards)
                {
                    // Skip selected
                    if (selectedLoyaltyReward != null && loyaltyReward.Id == selectedLoyaltyReward.Id)
                    {
                        continue;
                    }

                    itemList.Add(CreateLoyaltyRewardSelectListItem(loyaltyReward, loyaltyProgram, selected));

                    loyaltyRewards.Add(loyaltyReward);
                }
            }

            return itemList;
        }

        public static List<SelectListItem> NotificationsToSelectList()
        {
            return NotificationsToSelectList(selected: null, notificationDefinitions: out IList<IDtoNotificationDefinition> notificationDefinitions);
        }

        public static List<SelectListItem> NotificationsToSelectList(int? selected, out IList<IDtoNotificationDefinition> notificationDefinitions)
        {
            var itemList = new List<SelectListItem>();
            notificationDefinitions = new List<IDtoNotificationDefinition>();

            itemList.Add(new SelectListItem { Value = string.Empty, Text = "Choose a notification", Selected = !selected.HasValue });

            IDtoNotificationDefinition selectedNotificationDefinition = null;
            if (selected.HasValue)
            {
                selectedNotificationDefinition = _NotificationDefinitionRepository.Get(selected.Value);
                itemList.Add(CreateNotificationSelectListItem(selectedNotificationDefinition, selected));

                notificationDefinitions.Add(selectedNotificationDefinition);
            }

            IList<IDtoNotificationDefinition> allNotificationDefinitions = _NotificationDefinitionRepository.GetAll();
            foreach (IDtoNotificationDefinition notificationDefinition in allNotificationDefinitions)
            {
                // Skip selected
                if (selectedNotificationDefinition != null && notificationDefinition.Id == selectedNotificationDefinition.Id)
                {
                    continue;
                }

                itemList.Add(CreateNotificationSelectListItem(notificationDefinition, selected));

                notificationDefinitions.Add(notificationDefinition);
            }

            return itemList;
        }

        public void SetPageNavigationValuesForDetailPage(LoyaltyEventDefinitionModel model, UrlHelper urlHelper, int? page = null)
        {
            model.SetPageFields(page, PageTitle, urlHelper.Action(MVC.LoyaltyEventDefinition.ActionNames.Index), PageConst.Title_NavigateToPreviousPage);
        }

        private static SelectListItem CreateCampaignSelectListItem(Campaign campaign, string selected)
        {
            return new SelectListItem { Value = campaign.Id, Text = campaign.Title, Selected = campaign.Id == selected };
        }

        private static SelectListItem CreateLoyaltyProgramSelectListItem(LoyaltyPointsProgram loyaltyProgram, int? selected)
        {
            return new SelectListItem { Value = loyaltyProgram.Id.ToString(), Text = loyaltyProgram.Name, Selected = loyaltyProgram.Id == selected };
        }

        private static SelectListItem CreateLoyaltyRewardSelectListItem(LoyaltyPointsReward loyaltyReward, LoyaltyPointsProgram loyaltyProgram, int? selected)
        {
            return new SelectListItem
            {
                Value = loyaltyReward.Id.ToString(),
                Text = $"{loyaltyReward.Name} ({loyaltyProgram.Id})",
                Selected = loyaltyReward.Id == selected
            };
        }

        private static SelectListItem CreateNotificationSelectListItem(IDtoNotificationDefinition notificationDefinition, int? selected)
        {
            return new SelectListItem
            {
                Value = notificationDefinition.Id.ToString(),
                Text = notificationDefinition.Name,
                Selected = notificationDefinition.Id == selected
            };
        }
    }
}