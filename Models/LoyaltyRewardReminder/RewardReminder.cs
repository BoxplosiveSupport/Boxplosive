using System;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyRewardReminder
{
    public class RewardReminder : RewardReminderBase
    {
        public RewardReminder()
        {
        }

        public RewardReminder(IDtoLoyaltyPointsReward reward, LoyaltyRewardReminderType type)
        {
            Id = reward.Id;
            Type = type;
            CampaignId = reward.Campaign.Id;
            Title = reward.Campaign.Title;
            StartsAt = reward.StartsAt.ToLocalTimeInOperatingTimeZone();
            EndsAt = reward.EndsAt.HasValue ? reward.EndsAt.Value.ToLocalTimeInOperatingTimeZone() :
                DateTime.MaxValue.AddDays(-1).ToLocalTimeInOperatingTimeZone();
            Status = reward.Status;
        }

        public int Id { get; set; }

        public string CampaignId { get; set; }

        public string Title { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public LoyaltyPointsRewardStatus Status { get; set; }
    }
}