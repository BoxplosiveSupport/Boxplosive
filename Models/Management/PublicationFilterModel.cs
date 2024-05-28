using nl.boxplosive.Sdk;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.ComponentModel;
using System.Data.SqlTypes;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class PublicationFilterModel
    {
        public string Title { get; set; }

        [DisplayName("ID")]
        public string CampaignId { get; set; }

        [DisplayName("Visible from")]
        public DateTime? VisibleFrom { get; set; }

        [DisplayName("Starts at")]
        public DateTime? DateFrom { get; set; }

        [DisplayName("Ends at")]
        public DateTime? DateTill { get; set; }

        [DisplayName("Status")]
        public CampaignStatus? Status { get; set; }

        public DateTime StartsAt() => DateFrom.HasValue ? DateFrom.Value.ToLocalBeginOfDayInOperatingTimeZone() : SqlDateTime.MinValue.Value;

        public DateTime EndsAt() => DateTill.HasValue ? DateTill.Value.ToLocalEndOfDayInOperatingTimeZone() : SqlDateTime.MaxValue.Value;

        public bool HasFilters
        {
            get
            {
                return !string.IsNullOrEmpty(Title)
                    || !string.IsNullOrEmpty(CampaignId)
                    || VisibleFrom.HasValue
                    || DateFrom.HasValue
                    || DateTill.HasValue
                    || Status.HasValue;
            }
        }
    }
}