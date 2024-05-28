using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Reclamation;
using nl.boxplosive.Service.ServiceContract.CampaignService.DataContracts;
using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Service.ServiceModel.Customer;
using nl.boxplosive.Service.ServiceModel.Loyalty;
using nl.boxplosive.Utilities.Caching;
using nl.boxplosive.Utilities.Encryption;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class ReclamationFactory
    {
        private static string _CacheKey_CustomerId
        {
            get
            {
                Session session = AuthenticationHelpers.GetSession();
                return string.Format("CustomerId-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        private static int CustomerId
        {
            get
            {
                return DistributedCache.Instance.GetObject<int>(_CacheKey_CustomerId);
            }
        }

        private const int PageSize = 10;

        private static void SetReclamationModelPageProperties(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrl,
            ReclamationModel model)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationUrl = pageNavigationUrl;
            model.PageNavigationUrlText = pageNavigationLabel;
        }

        public static ReclamationModel CreateReclamationModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrl,
            ReclamationModel model = null)
        {
            if (model == null)
            {
                model = new ReclamationModel();
                SetReclamationModelPageProperties(pageTitle, pageNavigationLabel, pageNavigationUrl, model);
                return model;
            }

            bool hasValidCustomer = model.CustomerId > 0;
            bool hasValidSearchTerm = !string.IsNullOrWhiteSpace(model.SearchTerm);
            bool searchCustomer = hasValidCustomer || hasValidSearchTerm;
            if (!searchCustomer)
            {
                model = new ReclamationModel();
                SetReclamationModelPageProperties(pageTitle, pageNavigationLabel, pageNavigationUrl, model);
                return model;
            }

            SetReclamationModelPageProperties(pageTitle, pageNavigationLabel, pageNavigationUrl, model);

            string id = null;
            if (!hasValidCustomer && hasValidSearchTerm)
            {
                switch (model.SearchType)
                {
                    case CustomerDetailsType.CustomerExternalId:
                    case CustomerDetailsType.LoyaltyCardNumber:
                    default:
                        id = model.SearchTerm;
                        break;

                    case CustomerDetailsType.Username:
                        id = model.SearchTerm.Trim().ToLower().Encrypt(key: "\\Elmex\\nl.boxplosive.BackOffice.Mvc\\Factories\\ReclamationFactory.cs --> CreateReclamationModel() --> [customerProfile].[Value].[Username]");
                        break;
                }
            }

            var request = new GetCustomerDetailsRequest
            {
                CustomerId = model.CustomerId,
                Id = id,
                Type = model.SearchType,
                PageNumber = 1,
                PageSize = PageSize
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetDetails");
            var response = ServiceCallFactory.Customer_GetDetails(request);

            if (response.Success)
            {
                var details = response.CustomerDetails;
                DistributedCache.Instance.SetObject(_CacheKey_CustomerId, response.CustomerId, CacheSettings.DefaultRelativeExpiration_Backoffice);

                model.CustomerId = response.CustomerId;
                model.CustomerExternalId = details?.CustomerExternalId;
                model.LoyaltyCardNumber_Decrypted_MainNonOptional = details?.LoyaltyCardNumber_MainNonOptional?.Value;
                model.LoyaltyCardNumber_Decrypted_MainOptional = details?.LoyaltyCardNumber_MainOptional?.Value;
                model.Username_Decrypted = details?.Username?.Plaintext;

                SetCouponTab(model.CouponTab, details.Coupons, details.CouponsRowCount);

                SetLoyaltyTab(model.LoyaltyTab, details.LoyaltyPrograms, details.SelectedBalance, details.LoyaltyRowCount);
            }

            return model;
        }

        #region Coupons

        public static CouponTabModel CreateCouponTabModel(CouponTabModel model, int pageNumber)
        {
            if (model == null)
                model = new CouponTabModel();

            var request = new GetCustomerCouponsRequest
            {
                CustomerId = CustomerId,
                StatusFilter = model.Status,
                PageNumber = pageNumber,
                PageSize = PageSize
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetDetailsCoupons");
            var response = ServiceCallFactory.Customer_GetDetailsCoupons(request);

            if (response.Success)
            {
                SetCouponTab(model, response.Coupons, response.RowCount, pageNumber);
            }

            return model;
        }

        public static CouponTabModel AddCouponReservation(CouponTabModel model)
        {
            var request = new ReserveCouponRequest
            {
                CustomerId = CustomerId,
                CampaignId = model.SelectedCampaignId,
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Reserve");
            var response = ServiceCallFactory.Campaign_Reserve(request);
            if (response.Success)
            {
                model = CreateCouponTabModel(model, 1);
            }

            return model;
        }

        public static CouponTabModel ClaimCouponReservation(CouponTabModel model, int reservationId)
        {
            var request = new ClaimCouponRequest
            {
                CustomerId = CustomerId,
                ReservationId = reservationId
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Claim");
            var response = ServiceCallFactory.Campaign_Claim(request);
            if (response.Success)
            {
                model = CreateCouponTabModel(model, 1);
            }

            return model;
        }

        #endregion Coupons

        #region Loyalty

        public static LoyaltyTabModel CreateLoyaltyTabModel(LoyaltyTabModel model, int pageNumber)
        {
            if (model == null)
                model = new LoyaltyTabModel();

            var request = new GetCustomerLoyaltyProgramsRequest
            {
                CustomerId = CustomerId,
                ProgramIdFilter = model.SelectedLoyaltyProgram,
                PageNumber = pageNumber,
                PageSize = PageSize,
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetDetailsLoyaltyPrograms");
            var response = ServiceCallFactory.Customer_GetDetailsLoyaltyPrograms(request);

            if (response.Success)
            {
                SetLoyaltyTab(model, response.LoyaltyPrograms, response.SelectedBalance, response.RowCount, pageNumber);
            }

            return model;
        }

        public static LoyaltyTabModel AddLoyaltyMutation(LoyaltyTabModel model)
        {
            var request = new AddCustomerLoyaltyPointsMutationRequest
            {
                CustomerId = CustomerId,
                LoyaltyProgramId = model.SelectedLoyaltyProgram,
                Delta = model.MutationCreditDebitType == LoyaltyMutationCreditDebitType.Credit ? model.MutationAmount : -model.MutationAmount,
                Comment = model.Comment,
            };

            AuthenticationHelpers.SetupServiceRequest(request, "AddLoyaltyPointsMutation");
            var response = ServiceCallFactory.Customer_AddLoyaltyPointsMutation(request);
            if (response.Success)
            {
                // Recollect the tab
                model = CreateLoyaltyTabModel(model, 1);
            }

            return model;
        }

        #endregion Loyalty

        #region Helpers

        private static void SetCouponTab(CouponTabModel model, IEnumerable<CustomerCouponDetails> coupons, int itemCount, int pageNumber = 1)
        {
            if (coupons != null)
            {
                foreach (var coupon in coupons)
                {
                    model.Coupons = coupons.Select(c => new CouponModel
                    {
                        ReservationId = c.ReservationId ?? 0,
                        DateModified = c.LastModifiedDate,
                        Id = c.CouponCode,
                        PromotionName = c.CouponTitle,
                        Status = c.Status,
                        StartsAt = c.StartsAt,
                        EndsAt = c.EndsAt,
                        RedemptionsLeft = c.RedemptionsLeft
                    }).ToList();
                }
            }

            model.Pagination = new PaginationModel
            {
                PageNumber = pageNumber,
                ItemCount = itemCount,
                PageSize = PageSize
            };
        }

        private static void SetLoyaltyTab(LoyaltyTabModel model, IEnumerable<LoyaltyPointsProgram> loyaltyPrograms, LoyaltyPointsBalance balance, int itemCount,
            int pageNumber = 1)
        {
            model.LoyaltyPrograms = loyaltyPrograms.ToList()
                .Select(lp => new SelectListItem { Text = lp.Name, Value = lp.Id.ToString() })
                .ToList();

            if (balance != null)
            {
                model.SetBalanceInfo(balance.Subscribed, balance.PublicOrMember);
                model.TotalPoints = balance.Amount;
                model.LoyaltyMutations = balance.Mutations.Select(m =>
                    new MutationModel
                    {
                        DateIssued = m.CreatedAt,
                        MutationValue = m.Delta,
                        Source = m.Type.ToString(),
                        Comment = m.Comment,
                    }).ToList();
            }

            model.Pagination = new PaginationModel
            {
                PageNumber = pageNumber,
                ItemCount = itemCount,
                PageSize = PageSize
            };
        }

        #endregion Helpers
    }
}