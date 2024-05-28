using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Business.Sdk.Exceptions;
using nl.boxplosive.BackOffice.Business.Entities;
using nl.boxplosive.BackOffice.Business.Retailer;
using nl.boxplosive.Service;
using nl.boxplosive.Service.ServiceContract;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public static class BackOfficeBusinessCallFactory
    {
        // Note that service's 'BusinessApiInjector' can't be used here
        private static IRetailerStoreInfoApi RetailerStoreInfoApi = BusinessApiFactory.GetInstance().BusinessApi<IRetailerStoreInfoApi>();
        private static ILoyaltyPointsApi LoyaltyPointsApi = BusinessApiFactory.GetInstance().BusinessApi<ILoyaltyPointsApi>();

        /// <summary>
        ///
        /// </summary>
        private delegate void ServiceFunc();

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TReturnObject"></typeparam>
        /// <returns></returns>
        private delegate TReturnObject ServiceFunc<out TReturnObject>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="handler"></param>
        private static void CallHandler(string methodName, ServiceFunc handler)
        {
            CallHandler<object>(methodName, () => { handler(); return null; });
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="TObject"></typeparam>
        /// <param name="handler"></param>
        /// <returns></returns>
        private static TObject CallHandler<TObject>(string methodName, ServiceFunc<TObject> handler)
        {
            bool mustHackPrincipal = (HttpContext.Current.User as ClaimsPrincipal) != null;

            // Create request
            ServiceRequestBase request = GetRequest(methodName);

            if (mustHackPrincipal)
            {
                HttpContext.Current.Items["ClaimsPrincipal"] = HttpContext.Current.User;
                HttpContext.Current.User = BusinessApiInjector.GetInstance<IAccountInternalApi>().SetupPrincipal();
            }

            // Authenticate issuer
            request.ValidateClient();
            request.EnsureSession();

            // Execute call
            TObject result = handler();

            HttpContext.Current.User = (ClaimsPrincipal)HttpContext.Current.Items["ClaimsPrincipal"];
            HttpContext.Current.Items.Remove("ClaimsPrincipal");

            return result;
        }

        #region Loyalty API

        public static LoyaltyPointsBalance Loyalty_SubscribeToProgram(int programId)
        {
            return CallHandler("SubscribeToProgram", () =>
            {
                return LoyaltyPointsApi.SubscribeToProgram(programId);
            });
        }

        public static void Loyalty_UnsubscribeFromProgram(int programId)
        {
            CallHandler("UnsubscribeFromProgram", () =>
            {
                LoyaltyPointsApi.UnsubscribeFromProgram(programId);
            });
        }

        #endregion Loyalty API

        #region Retailer API

        public static List<RetailerStore> Retailer_GetRetailerStores(string loginName)
        {
            return CallHandler("GetRetailerStores", () =>
            {
                return RetailerStoreInfoApi.GetRetailerStores(loginName);
            });
        }

        public static RetailerStore Retailer_GetRetailerStore(int retailerId, int retailerStoreId)
        {
            return CallHandler("GetRetailerStore", () =>
            {
                return RetailerStoreInfoApi.GetRetailerStore(retailerId, retailerStoreId);
            });
        }

        public static int Retailer_SaveRetailerStore(RetailerStore store, string password)
        {
            return CallHandler("SaveRetailerStore", () =>
            {
                return RetailerStoreInfoApi.SaveRetailerStore(store, password);
            });
        }

        public static void Retailer_DeleteRetailerStore(RetailerStore store)
        {
            CallHandler("DeleteRetailerStore", () =>
            {
                RetailerStoreInfoApi.DeleteRetailerStore(store);
            });
        }

        public static List<Store> Retailer_SearchRetailerStores(double latitude, double longitude, int range, string[] retailerLogins)
        {
            return CallHandler("SearchRetailerStores", () =>
            {
                return RetailerStoreInfoApi.SearchRetailerStores(latitude, longitude, range, retailerLogins);
            });
        }

        #endregion Retailer API

        #region Helpers

        /// <summary>
        ///
        /// </summary>
        /// <param name="methodName"></param>
        /// <returns></returns>
        private static ServiceRequestBase GetRequest(string methodName)
        {
            var request = new BackOfficeBusinessApiRequest();
            AuthenticationHelpers.SetupServiceRequest(request, methodName);

            return request;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        private static void ValidateClient(this ServiceRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            // Validate client
            int apiClientId = request.ApiClientId;

            // Authenticate API client
            var apiClientApi = BusinessApiInjector.GetInstance<IApiClientApi>();
            apiClientApi.Authenticate(apiClientId);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="request"></param>
        private static void EnsureSession(this ServiceRequestBase request)
        {
            if (request == null)
                throw new ArgumentNullException("request");

            if (request.Session == null)
                throw new SessionTicketNotValidException(null);

            if (string.IsNullOrEmpty(request.Session.SessionTicket))
                throw new SessionTicketNotValidException(request.Session.SessionTicket);

            var sessionApi = BusinessApiInjector.GetInstance<ISessionApi>();

            sessionApi.Load(request.Session.SessionTicket);
        }

        private class BackOfficeBusinessApiRequest : ServiceRequestBase
        {
        }

        #endregion Helpers
    }
}