using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;
using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class CouponTabModel : TabModelBase
    {
        public CouponTabModel()
        {
            Coupons = new List<CouponModel>();
        }

        public GetCustomerCouponsRequest.CouponStatus? Status { get; set; }

        public string SelectedCampaignId { get; set; }

        public List<CouponModel> Coupons { get; set; }

        public CouponModel Coupon { get; set; }

        public int TotalReserved { get; set; }
        public int TotalClaimed { get; set; }
        public int TotalCleared { get; set; }

        public ReclamationSearchValueModel SearchValueModel { get; set; }
    }
}