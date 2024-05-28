using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.ComponentModel;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class PromotionTimeframeStepModel : TimeframeStepModel
    {
        public PromotionTimeframeStepModel()
        {
        }

        public PromotionTimeframeStepModel(PromotionCampaign serviceModel)
            : this()
        {
            StartDate = serviceModel.StartDate.ToLocalTimeInOperatingTimeZone().Date;
            StartTime = serviceModel.StartDate.ToLocalTimeInOperatingTimeZone().TimeOfDay;
            EndDate = serviceModel.EndDate.ToLocalTimeInOperatingTimeZone().Date;
            EndTime = serviceModel.EndDate.ToLocalTimeInOperatingTimeZone().TimeOfDay;
            VisibleFromDate = serviceModel.VisibleFrom.ToLocalTimeInOperatingTimeZone().Date;
            VisibleFromTime = serviceModel.VisibleFrom.ToLocalTimeInOperatingTimeZone().TimeOfDay;
        }

        [DisplayName("Visible from")]
        public DateTime VisibleFromDate { get; set; }

        public TimeSpan VisibleFromTime { get; set; }
    }
}