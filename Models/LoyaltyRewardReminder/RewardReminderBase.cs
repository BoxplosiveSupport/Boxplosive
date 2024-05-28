using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using nl.boxplosive.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyRewardReminder
{
    public abstract class RewardReminderBase
    {
        public RewardReminderBase()
        {
        }

        public LoyaltyRewardReminderType Type { get; set; }

        public string TypeText
        {
            get
            {
                switch (Type)
                {
                    case LoyaltyRewardReminderType.NewRewardEmail:
                        return "New reward email";

                    case LoyaltyRewardReminderType.AlmostExpiredEmail:
                        return "Almost expired reward email";

                    case LoyaltyRewardReminderType.AlmostExpiredLoyaltyEvent:
                        return "Almost expired reward loyalty event";

                    default:
                        return Type.ToString();
                }
            }
        }
    }
}