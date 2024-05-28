using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class LoyaltyModel
    {
        public LoyaltyModel()
            : this(null)
        {
        }

        public LoyaltyModel(LoyaltyCampaign campaign)
        {            
            Basics = campaign != null ? new BasicStepModel(campaign) : new BasicStepModel();
            Timeframe = new TimeframeStepModel();
            if (campaign != null)
            {
                Timeframe.StartDate = campaign.StartDate.ToLocalTimeInOperatingTimeZone().Date;
                Timeframe.EndDate = campaign.EndDate.ToLocalTimeInOperatingTimeZone().Date;
                Timeframe.StartTime = campaign.StartDate.ToLocalTimeInOperatingTimeZone().TimeOfDay;
                Timeframe.EndTime = campaign.EndDate.ToLocalTimeInOperatingTimeZone().TimeOfDay;
            }
            Rewards = campaign != null ? LoyaltyFactory.CreateRewardsStepModel(campaign) : new RewardsStepModel();
            Savings = campaign != null ? LoyaltyFactory.CreateSavingsStepModel(campaign) : new SavingsStepModel();
        }

        public int Id { get; set; }

        public BasicStepModel Basics { get; set; }
        public TimeframeStepModel Timeframe { get; set; }
        public RewardsStepModel Rewards { get; set; }
        public SavingsStepModel Savings { get; set; }        
    }
}