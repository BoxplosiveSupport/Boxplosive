using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Reclamation;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Business.Sdk.Exceptions;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Service.ServiceModel.Customer;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Linq;
using System.Web.Mvc;
using ServiceDataContracts = nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "PermissionReclamationView")]
    public partial class ReclamationController : ControllerBase
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        #region APIs

        protected ICampaignApi CampaignApi = BusinessApiFactory.GetInstance().BusinessApi<ICampaignApi>();

        #endregion APIs

        #region Repositories

        protected ICouponReservationRepository CouponReservationRepository =
            DataRepositoryFactory.GetInstance().DataRepository<ICouponReservationRepository>();

        #endregion Repositories

        public delegate ActionResult IndexDelegate(int? customerId = null, string searchType = null, string searchTerm = null);

        // GET: Reclamation
        public virtual ActionResult Index(int? customerId = null, string searchType = null, string searchTerm = null)
        {
            ReclamationModel searchModel = null;
            if (customerId.HasValue || (!string.IsNullOrWhiteSpace(searchType) && !string.IsNullOrWhiteSpace(searchType)))
            {
                if (Enum.TryParse(searchType, ignoreCase: true, out CustomerDetailsType searchTypeEnum))
                    searchModel = new ReclamationModel() { CustomerId = customerId ?? -1, SearchType = searchTypeEnum, SearchTerm = searchTerm };
            }

            var model = CreateReclamationModel(searchModel);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Index(ReclamationModel model)
        {
            model = CreateReclamationModel(model);

            return View(model);
        }

        public virtual ActionResult DeleteMainOptionalLoyaltyCard(int customerId, string searchType, string searchTerm)
        {
            LoyaltyCard deletedLoyaltyCard = null;
            try
            {
                Customer customer = CustomerApi.Get(customerId);
                LoyaltyCard loyaltyCard =
                    CustomerApi.GetLoyaltyCards(customer).FirstOrDefault(item => item.DtoLoyaltyCard.IsMainCard && item.DtoLoyaltyCard.IsOptionalCard);
                CustomerApi.DeleteOptionalLoyaltyCard(customer, loyaltyCard.Number, loyaltyCard.Type);

                deletedLoyaltyCard = loyaltyCard;
            }
            catch (Exception ex)
            {
                if (_Logger.IsErrorEnabled)
                    _Logger.Error(ex, $"Failed to delete physical loyalty card: customerId={customerId}");
            }

            if (deletedLoyaltyCard != null)
                TempData["Success"] = $"Physical loyalty card '{deletedLoyaltyCard?.Number?.Value}' deleted.";
            else
                TempData["Danger"] = "Failed to delete physical loyalty card.";

            return RedirectToAction(MVC.Reclamation.ActionNames.Index, new { customerId = deletedLoyaltyCard != null ? customerId : default(int?), searchType = searchType, searchTerm = searchTerm });
        }

        public virtual ActionResult CouponsTab(CouponTabModel model, string id, int? page)
        {
            var pageNumber = 1;
            if (page.HasValue && page.Value > 1)
                pageNumber = page.Value;

            model = ReclamationFactory.CreateCouponTabModel(model, pageNumber);
            return ReturnCorrectView(id, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, ReclamationManager")]
        public virtual ActionResult CouponsTabAddReservation(CouponTabModel model, string id)
        {
            model = ReclamationFactory.AddCouponReservation(model);
            return ReturnCorrectView(id, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, ReclamationEmployee, ReclamationManager")]
        public virtual ActionResult CouponsTabClaim(CouponTabModel model, string id, int reservationId)
        {
            model = ReclamationFactory.ClaimCouponReservation(model, reservationId);
            return ReturnCorrectView(id, model);
        }

        public virtual ActionResult LoyaltyTab(LoyaltyTabModel model, string id, int? page)
        {
            var pageNumber = 1;
            if (page.HasValue && page.Value > 1)
                pageNumber = page.Value;

            model = ReclamationFactory.CreateLoyaltyTabModel(model, pageNumber);
            return ReturnCorrectView(id, model);
        }

        public delegate ActionResult LoyaltyTabDelegate(LoyaltyTabModel model, string id);

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, ReclamationEmployee, ReclamationManager")]
        public virtual ActionResult LoyaltyTab(LoyaltyTabModel model, string id)
        {
            model = ReclamationFactory.AddLoyaltyMutation(model);
            return ReturnCorrectView(id, model);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, ReclamationManager")]
        public virtual ActionResult EditCoupon(int reservationId, int customerId, CustomerDetailsType searchType, string searchTerm)
        {
            var couponReservation = CouponReservationRepository.Get(reservationId);
            if (couponReservation == null)
            {
                TempData["Danger"] = string.Format("No reservation found");
                return Redirect(Request.UrlReferrer.ToString());
            }

            var model = new EditCouponModel()
            {
                ReservationId = reservationId,
                ExternalCustomerId = couponReservation.Customer.ExternalId,
                CustomerId = couponReservation.Customer.Id,
                CampaignId = couponReservation.Campaign.Id,
                CampaignName = couponReservation.Campaign.Title,
                Status = GetCouponStatus(couponReservation.Status),
                StartDate = couponReservation.StartsAt.ToLocalTimeInOperatingTimeZone().Date,
                StartTime = couponReservation.StartsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                EndDate = couponReservation.EndsAt.ToLocalTimeInOperatingTimeZone().Date,
                EndTime = couponReservation.EndsAt.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                RedemptionsLeft = couponReservation.RedemptionsLeft,
                SearchValueModel = new ReclamationSearchValueModel(customerId, searchType, searchTerm, selectedTab: "Coupon"),
            };

            return View(model);
        }

        private ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus GetCouponStatus(Sdk.CouponReservationStatus status)
        {
            switch (status)
            {
                case Sdk.CouponReservationStatus.Active:
                    return ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus.Reserved;

                case Sdk.CouponReservationStatus.Claimed:
                    return ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus.Claimed;
            }

            return ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus.Inactive;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, ReclamationManager")]
        public virtual ActionResult EditCoupon(EditCouponModel model)
        {
            if (model.Status != ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus.Reserved
                && model.Status != ServiceDataContracts.GetCustomerCouponsRequest.CouponStatus.Claimed)
            {
                TempData["Danger"] = string.Format("The coupon reservation may only be updated when the status is Reserved or Claimed");
                return View(model);
            }

            DateTime startsAt = model.StartDate.Add(model.StartTime);
            DateTime endsAt = model.EndDate.Add(model.EndTime);
            DateTime now = OperatingDateTime.NowLocal.FloorByMinute();

            if (endsAt < now || endsAt < startsAt)
            {
                ViewData["EndDateTimeError"] = "The coupon cannot end in the past";
                return View(model);
            }

            var couponReservation = new CouponReservation(CouponReservationRepository.Get(model.ReservationId));
            couponReservation.EndsAt = endsAt.ToUtcInOperatingTimeZone();
            couponReservation.SpecificRedemptionPeriod = true;
            couponReservation.RedemptionsLeft = model.RedemptionsLeft;

            bool saved = false;
            try
            {
                saved = CampaignApi.UpdateCouponReservation(couponReservation) != null;
            }
            catch (CustomerLockedException ex)
            {
                TempData["Danger"] = "The customer is locked, please try it again";
                return View(model);
            }

            if (!saved)
            {
                TempData["Danger"] = "The coupon reservation has not been updated";
                return View(model);
            }

            TempData["Success"] = "The coupon reservation has been updated";

            return ReturnToCurrentReclamation(model.SearchValueModel.CustomerId, model.SearchValueModel.SearchType, model.SearchValueModel.SearchTerm, model.SearchValueModel.SelectedTab);
        }

        public virtual ViewResult ReturnToCurrentReclamation(int customerId, CustomerDetailsType searchType, string searchTerm, string selectedTab = null)
        {
            var model = new ReclamationModel()
            {
                CustomerId = customerId,
                SearchType = searchType,
                SearchTerm = searchTerm,
                SelectedTab = selectedTab,
            };

            var reclamationModel = CreateReclamationModel(model);

            return View("Index", reclamationModel);
        }

        private ReclamationModel CreateReclamationModel(ReclamationModel model = null)
        {
            return ReclamationFactory.CreateReclamationModel("Reclamation", PageConst.Title_NavigateToManagementPage,
                Url.Action(MVC.Management.ActionNames.Index, MVC.Management.Name),
                model);
        }

        private ActionResult ReturnCorrectView(string id, TabModelBase model)
        {
            switch (id.ToLower())
            {
                case "reclamation-coupons": // Step 1
                    return PartialView(MVC.Reclamation.Views.DisplayTemplates.CouponTabModel, (CouponTabModel)model);

                case "reclamation-loyaltyprograms": // Step 1
                    return PartialView(MVC.Reclamation.Views.DisplayTemplates.LoyaltyTabModel, (LoyaltyTabModel)model);

                default:
                    return null;
            }
        }
    }
}