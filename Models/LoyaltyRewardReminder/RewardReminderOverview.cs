using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyRewardReminder
{
    public class RewardReminderOverview
    {
        public RewardReminderOverview()
        {
            Rewards = new List<RewardReminder>();
        }

        public RewardReminderOverview(int programId, IEnumerable<IDtoLoyaltyPointsReward> dtoRewards)
        {
            LoyaltyPointsProgramId = programId;
            Rewards = new List<RewardReminder>();

            // Create for every reward a different reward reminder
            foreach (var reward in dtoRewards)
            {
                Rewards.Add(new RewardReminder(reward, LoyaltyRewardReminderType.NewRewardEmail));
                Rewards.Add(new RewardReminder(reward, LoyaltyRewardReminderType.AlmostExpiredEmail));
                Rewards.Add(new RewardReminder(reward, LoyaltyRewardReminderType.AlmostExpiredLoyaltyEvent));
            }
        }

        public int LoyaltyPointsProgramId { get; set; }

        public IList<RewardReminder> Rewards { get; set; }
    }
}