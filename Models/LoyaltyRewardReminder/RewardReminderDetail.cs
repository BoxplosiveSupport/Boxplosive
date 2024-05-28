using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyRewardReminder
{
    public class RewardReminderDetail : RewardReminderBase
    {
        public RewardReminderDetail()
            : base()
        {
        }

        public RewardReminderDetail(IDtoLoyaltyPointsReward reward, LoyaltyRewardReminderType type)
            : this()
        {
            Type = type;
            FinalRunAt = reward.EndsAt.HasValue ? reward.EndsAt.Value.ToLocalTimeInOperatingTimeZone() : OperatingDateTime.NowLocal.AddDays(7);
            LoyaltyPointsRewardId = reward.Id;
        }

        public RewardReminderDetail(IDtoLoyaltyRewardReminder iDtoLoyaltyRewardReminder)
        {
            ReminderId = iDtoLoyaltyRewardReminder.Id;
            Type = iDtoLoyaltyRewardReminder.Type;
            PreviousRunAtDate = iDtoLoyaltyRewardReminder.PreviousRunAt.HasValue ? iDtoLoyaltyRewardReminder.PreviousRunAt.Value.ToLocalTimeInOperatingTimeZone().Date : (DateTime?)null;
            PreviousRunAtTime = iDtoLoyaltyRewardReminder.PreviousRunAt.HasValue ? iDtoLoyaltyRewardReminder.PreviousRunAt.Value.ToLocalTimeInOperatingTimeZone().TimeOfDay : (TimeSpan?)null;
            NextRunAtDate = iDtoLoyaltyRewardReminder.NextRunAt.HasValue ? iDtoLoyaltyRewardReminder.NextRunAt.Value.ToLocalTimeInOperatingTimeZone().Date : (DateTime?)null;
            NextRunAtTime = iDtoLoyaltyRewardReminder.NextRunAt.HasValue ? iDtoLoyaltyRewardReminder.NextRunAt.Value.ToLocalTimeInOperatingTimeZone().TimeOfDay : (TimeSpan?)null;
            FinalRunAt = iDtoLoyaltyRewardReminder.FinalRunAt.ToLocalTimeInOperatingTimeZone();
            Frequency = iDtoLoyaltyRewardReminder.Frequency.HasValue ? iDtoLoyaltyRewardReminder.Frequency.Value / (86400) : iDtoLoyaltyRewardReminder.Frequency; // Convert seconds to days
            TargetGroup = iDtoLoyaltyRewardReminder.TargetGroup;
            IsEnabled = iDtoLoyaltyRewardReminder.IsEnabled;
            LoyaltyPointsRewardId = iDtoLoyaltyRewardReminder.LoyaltyPointsRewardId;
            DaysBeforeExpiry = iDtoLoyaltyRewardReminder.AlmostExpiredReminderPeriod.ToNullableIntDays();
        }

        public IDtoLoyaltyRewardReminder ToDto()
        {
            return new DtoLoyaltyRewardReminder
            {
                Id = ReminderId,
                Type = Type,
                PreviousRunAt = PreviousRunAtDate.HasValue ? PreviousRunAtDate.Value.Date.Add(PreviousRunAtTime ?? default(TimeSpan)).ToUtcInOperatingTimeZone() : (DateTime?)null,
                NextRunAt = NextRunAtDate.HasValue ? NextRunAtDate.Value.Date.Add(NextRunAtTime ?? default(TimeSpan)).ToUtcInOperatingTimeZone() : (DateTime?)null,
                FinalRunAt = FinalRunAt.ToUtcInOperatingTimeZone(),
                Frequency = Frequency.HasValue ? Frequency.Value * (86400) : Frequency, // Convert days to seconds
                TargetGroup = TargetGroup,
                IsEnabled = IsEnabled,
                LoyaltyPointsRewardId = LoyaltyPointsRewardId,
                AlmostExpiredReminderPeriod = DaysBeforeExpiry.DaysToNullableTimeSpan(),
            };
        }

        /// <summary>
        /// The ID of the reward reminder.
        /// </summary>
        /// <remarks>
        /// Note that this is not called 'Id' because that gives interference with the MVC routes on Id.
        /// </remarks>
        public int ReminderId { get; set; }

        [DisplayName("Previous run")]
        public DateTime? PreviousRunAtDate { get; set; }

        [DisplayName("Time")]
        public TimeSpan? PreviousRunAtTime { get; set; }

        [DisplayName("Next run")]
        public DateTime? NextRunAtDate { get; set; }

        [DisplayName("Time")]
        public TimeSpan? NextRunAtTime { get; set; }

        [DisplayName("Final run")]
        [Required]
        public DateTime FinalRunAt { get; set; }

        [DisplayName("Frequency (days)")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int? Frequency { get; set; }

        [DisplayName("Days before expiry")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int? DaysBeforeExpiry { get; set; }

        [DisplayName("Target groupcode")]
        public string TargetGroup { get; set; }

        [DisplayName("Enable")]
        public bool IsEnabled { get; set; }

        public int LoyaltyPointsRewardId { get; set; }
    }
}