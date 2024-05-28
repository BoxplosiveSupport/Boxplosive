using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Service;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ServiceContract.AuthenticationService;
using nl.boxplosive.Service.ServiceContract.AuthenticationService.DataContracts;
using nl.boxplosive.Service.ServiceContract.CampaignService;
using nl.boxplosive.Service.ServiceContract.CampaignService.DataContracts;
using nl.boxplosive.Service.ServiceContract.CronJobService;
using nl.boxplosive.Service.ServiceContract.CronJobService.DataContracts;
using nl.boxplosive.Service.ServiceContract.CustomerService;
using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;
using nl.boxplosive.Service.ServiceContract.FileService;
using nl.boxplosive.Service.ServiceContract.FileService.DataContracts;
using nl.boxplosive.Service.ServiceContract.IssuerService;
using nl.boxplosive.Service.ServiceContract.IssuerService.DataContracts;
using nl.boxplosive.Service.ServiceContract.LoyaltyPointService;
using nl.boxplosive.Service.ServiceContract.LoyaltyPointService.DataContracts;
using nl.boxplosive.Service.ServiceContract.ProductService;
using nl.boxplosive.Service.ServiceContract.ProductService.DataContracts;
using nl.boxplosive.Service.ServiceContract.PublicationService;
using nl.boxplosive.Service.ServiceContract.PublicationService.DataContracts;
using nl.boxplosive.Service.ServiceContract.ReportingService;
using nl.boxplosive.Service.ServiceContract.ReportingService.DataContracts;
using System.Security.Claims;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public static class ServiceCallFactory
    {
        private static IAuthenticationService AuthenticationService = ServiceInjector.GetInstance<IAuthenticationService>();
        private static ICampaignService CampaignService = ServiceInjector.GetInstance<ICampaignService>();
        private static ICampaignProcessService CampaignProcessService = ServiceInjector.GetInstance<ICampaignProcessService>();
        private static ICronJobService CronJobService = ServiceInjector.GetInstance<ICronJobService>();
        private static ICustomerService CustomerService = ServiceInjector.GetInstance<ICustomerService>();
        private static IFileService FileService = ServiceInjector.GetInstance<IFileService>();
        private static IIssuerService IssuerService = ServiceInjector.GetInstance<IIssuerService>();
        private static ILoyaltyPointService LoyaltyPointService = ServiceInjector.GetInstance<ILoyaltyPointService>();
        private static IProductService ProductService = ServiceInjector.GetInstance<IProductService>();
        private static IPublicationService PublicationService = ServiceInjector.GetInstance<IPublicationService>();
        private static IReportingService ReportingService = ServiceInjector.GetInstance<IReportingService>();

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TReturnObject"></typeparam>
        /// <returns></returns>
        private delegate TReturnObject ServiceFunc<out TReturnObject>();

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        private static TObject CallHandler<TObject>(ServiceFunc<TObject> handler)
        {
            bool mustHackPrincipal = (HttpContext.Current.User as ClaimsPrincipal) != null;

            TObject result = default(TObject);
            try
            {
                if (mustHackPrincipal)
                {
                    HttpContext.Current.Items["ClaimsPrincipal"] = HttpContext.Current.User;
                    HttpContext.Current.User = BusinessApiInjector.GetInstance<IAccountInternalApi>().SetupPrincipal();
                }

                result = handler();
            }
            finally
            {
                if (mustHackPrincipal)
                {
                    HttpContext.Current.User = (ClaimsPrincipal)HttpContext.Current.Items["ClaimsPrincipal"];
                    HttpContext.Current.Items.Remove("ClaimsPrincipal");
                }
            }

            return result;
        }

        #region Authentication service

        public static LoginResponse Authentication_Login(LoginRequest request)
        {
            return CallHandler(() =>
            {
                return AuthenticationService.Login(request);
            });
        }

        #endregion Authentication service

        #region CampaignProcess service

        public static CampaignProcessResponse CampaignProcess_Create(CampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.Create(request);
            });
        }

        public static GetCampaignProcessResponse CampaignProcess_Get(GetCampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.Get(request);
            });
        }

        public static GetAllCampaignProcessResponse CampaignProcess_GetAll(GetAllCampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.GetAll(request);
            });
        }

        public static PublishCampaignProcessResponse CampaignProcess_Publish(CampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.Publish(request);
            });
        }

        public static CampaignProcessResponse CampaignProcess_Modify(CampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.Modify(request);
            });
        }

        public static DeleteCampaignProcessResponse CampaignProcess_Delete(DeleteCampaignProcessRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignProcessService.Delete(request);
            });
        }

        #endregion CampaignProcess service

        #region Campaign service

        public static GetCampaignResponse Campaign_Get(GetCampaignRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignService.Get(request);
            });
        }

        public static ReserveCouponResponse Campaign_Reserve(ReserveCouponRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignService.Reserve(request);
            });
        }

        public static ClaimCouponResponse Campaign_Claim(ClaimCouponRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignService.Claim(request);
            });
        }

        public static GetUnexpiredCampaignResponse Campaign_GetUnexpired(GetUnexpiredCampaignRequest request)
        {
            return CallHandler(() =>
            {
                return CampaignService.GetUnexpired(request);
            });
        }

        #endregion Campaign service

        #region File service

        public static SaveFileResponse File_Save(SaveFileRequest request)
        {
            return CallHandler(() =>
            {
                return FileService.Save(request);
            });
        }

        #endregion File service

        #region Product service

        public static GetProductGroupsResponse Product_GetProductGroups(GetProductGroupsRequest request)
        {
            return CallHandler(() =>
            {
                return ProductService.GetProductGroups(request);
            });
        }

        public static GetProductsByGtinResponse Product_GetProductsByGtin(GetProductsByGtinRequest request)
        {
            return CallHandler(() =>
            {
                return ProductService.GetProductsByGtin(request);
            });
        }

        public static GetProductsResponse Product_GetProducts(GetProductsRequest request)
        {
            return CallHandler(() =>
            {
                return ProductService.GetProducts(request);
            });
        }

        public static GetProductsWithValidationResponse Product_GetProductsWithValidation(GetProductsWithValidationRequest request)
        {
            return CallHandler(() =>
            {
                return ProductService.GetProductsWithValidation(request);
            });
        }

        #endregion Product service

        #region Publication service

        public static GetAllFilteredPublicationResponse Publication_GetAllFiltered(GetAllFilteredPublicationRequest request)
        {
            return CallHandler(() =>
            {
                return PublicationService.GetAllFiltered(request);
            });
        }

        public static GetCountPublicationResponse Publication_GetCount(GetCountPublicationRequest request)
        {
            return CallHandler(() =>
            {
                return PublicationService.GetCount(request);
            });
        }

        #endregion Publication service

        #region Issuer service

        public static CreateOrUpdateIssuerResponse IssuerService_CreateOrUpdateIssuer(CreateOrUpdateIssuerRequest request)
        {
            return CallHandler(() =>
            {
                return IssuerService.CreateOrUpdateIssuer(request);
            });
        }

        #endregion Issuer service

        #region LoyaltyPoints Service

        public static GetLoyaltyPointsProgramsResponse LoyaltyPoints_GetPrograms(GetLoyaltyPointsProgramsRequest request)
        {
            return CallHandler(() =>
            {
                return LoyaltyPointService.GetLoyaltyPointsPrograms(request);
            });
        }

        public static GetFilteredLoyaltyPointsProgramsResponse LoyaltyPoints_GetFilteredPrograms(GetFilteredLoyaltyPointsProgramsRequest request)
        {
            return CallHandler(() =>
            {
                return LoyaltyPointService.GetFilteredLoyaltyPointsPrograms(request);
            });
        }

        public static GetFilteredLoyaltyPointsProgramsCountResponse LoyaltyPoints_GetFilteredProgramsCount(GetFilteredLoyaltyPointsProgramsCountRequest request)
        {
            return CallHandler(() =>
            {
                return LoyaltyPointService.GetFilteredLoyaltyPointsProgramsCount(request);
            });
        }

        #endregion LoyaltyPoints Service

        #region Analyses

        public static GetBasicStatisticsResponse ReportingService_GetBasicStatistics(GetBasicStatisticsRequest request)
        {
            return CallHandler(() =>
            {
                return ReportingService.GetBasicStatistics(request);
            });
        }

        public static GetLiveOrFinishedCampaignsResponse ReportingService_GetLiveOrFinishedCampaigns(GetLiveOrFinishedCampaignsRequest request)
        {
            return CallHandler(() =>
            {
                return ReportingService.GetLiveOrFinishedCampaigns(request);
            });
        }

        #endregion Analyses

        #region CronJob Service

        public static GetAllCronJobResponse CronJob_GetAll(GetAllCronJobRequest request)
        {
            return CallHandler(() =>
            {
                return CronJobService.GetAll(request);
            });
        }

        #endregion CronJob Service

        #region Customer Service

        public static GetCustomerDetailsResponse Customer_GetDetails(GetCustomerDetailsRequest request)
        {
            return CallHandler(() =>
            {
                return CustomerService.GetDetails(request);
            });
        }

        public static GetCustomerCouponsResponse Customer_GetDetailsCoupons(GetCustomerCouponsRequest request)
        {
            return CallHandler(() =>
            {
                return CustomerService.GetDetailsCoupons(request);
            });
        }

        public static GetCustomerLoyaltyProgramsResponse Customer_GetDetailsLoyaltyPrograms(GetCustomerLoyaltyProgramsRequest request)
        {
            return CallHandler(() =>
            {
                return CustomerService.GetDetailsLoyaltyPrograms(request);
            });
        }

        public static GetCustomersWithValidationResponse Customer_GetMultipleWithValidation(GetCustomersWithValidationRequest request)
        {
            return CallHandler(() =>
            {
                return CustomerService.GetMultipleWithValidation(request);
            });
        }

        public static AddCustomerLoyaltyPointsMutationResponse Customer_AddLoyaltyPointsMutation(
            AddCustomerLoyaltyPointsMutationRequest request)
        {
            return CallHandler(() =>
            {
                return CustomerService.AddLoyaltyPointsMutation(request);
            });
        }

        #endregion Customer Service
    }
}