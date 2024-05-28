using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;
using System;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class EditCouponModel
    {
        [Display(Name = "ID")]
        public int ReservationId { get; set; }

        public string ExternalCustomerId { get; set; }

        public int CustomerId { get; set; }

        [Display(Name = "Campaign")]
        public string CampaignId { get; set; }

        public string CampaignName { get; set; }

        public GetCustomerCouponsRequest.CouponStatus Status { get; set; }

        [Display(Name = "Start")]
        public DateTime StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        [Display(Name = "End")]
        public DateTime EndDate { get; set; }

        public TimeSpan EndTime { get; set; }

        [Display(Name = "Status")]
        public string StatusString
        {
            get { return Status.ToString(); }
        }

        [Display(Name = "Redemptions left")]
        [Range(1, int.MaxValue, ErrorMessage = ValidationHelper.ValueAtLeast1Message)]
        public int RedemptionsLeft { get; set; }

        public ReclamationSearchValueModel SearchValueModel { get; set; }
    }
}