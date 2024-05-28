using System;
using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class CouponModel
    {
        public int ReservationId { get; set; }

        public string Id { get; set; }

        public DateTime? DateModified { get; set; }

        public string PromotionName { get; set; }

        public GetCustomerCouponsRequest.CouponStatus Status { get; set; }

        public DateTime StartsAt { get; set; }

        public DateTime EndsAt { get; set; }

        public int RedemptionsLeft { get; set; }
    }
}