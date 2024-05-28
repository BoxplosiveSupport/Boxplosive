using System;
using System.ComponentModel;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
    public class TimeframeStepModel : StepModelBase
    {
        public TimeframeStepModel()
        {
            StartTime = new TimeSpan(7, 0, 0);
            EndTime = new TimeSpan(17, 0, 0);
        }

        [DisplayName("Start")]
        public DateTime StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        [DisplayName("End")]
        public DateTime EndDate { get; set; }

        public TimeSpan EndTime { get; set; }
    }
}