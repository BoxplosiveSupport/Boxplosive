using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.Data.Sdk.Enums;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ServiceContract.ProductService.DataContracts;
using nl.boxplosive.Service.ServiceContract.PublicationService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Utilities.Caching;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public class PlatformHelper
    {
        private static string _CacheKey_Promotions(string searchTerm)
        {
            Session session = AuthenticationHelpers.GetSession();
            return string.Format("Promotions-{0}-{1}", (session != null) ? session.SessionTicket : "", searchTerm);
        }

        #region Products

        public static IEnumerable<Product> GetProducts(string searchTerm, string groupName)
        {
            var request = new GetProductsRequest
            {
                SearchTerm = searchTerm,
                GroupName = groupName
            };
            AuthenticationHelpers.SetupServiceRequest(request, "GetProducts");

            var response = ServiceCallFactory.Product_GetProducts(request);
            return response.Products;
        }

        public static IEnumerable<Product> GetProductsByGtin(ICollection<string> gtinList)
        {
            var request = new GetProductsByGtinRequest
            {
                GtinList = gtinList
            };
            AuthenticationHelpers.SetupServiceRequest(request, "GetProductsByGtin");

            var response = ServiceCallFactory.Product_GetProductsByGtin(request);
            return response.Products;
        }

        public static IEnumerable<ProductGroup> GetProductGroups(string searchTerm)
        {
            var request = new GetProductGroupsRequest
            {
                SearchTerm = searchTerm,
            };
            AuthenticationHelpers.SetupServiceRequest(request, "GetProductGroups");

            var response = ServiceCallFactory.Product_GetProductGroups(request);
            return response.ProductGroups;
        }

        #endregion Products

        #region Promotions

        public static List<PromotionCampaign> GetPromotions(string searchTerm)
        {
            searchTerm = searchTerm.ToUpper();

            var campaigns = DistributedCache.Instance.GetObject<List<PromotionCampaign>>(_CacheKey_Promotions(searchTerm));

            // Check if the time-out expired
            if (campaigns == null)
            {
                // Collect data from platform
                var request = new GetAllFilteredPublicationRequest
                {
                    PageNumber = 1,
                    PageSize = 100,
                    StartsAt = SqlDateTime.MinValue.Value,
                    EndsAt = SqlDateTime.MaxValue.Value,
                    Status = CampaignStatus.Active,
                    SearchTerm = searchTerm,
                    FilterType = CampaignFilterType.Promotion,
                };

                AuthenticationHelpers.SetupServiceRequest(request, "GetAll");

                var response = ServiceCallFactory.Publication_GetAllFiltered(request);
                if (response.Success)
                {
                    // Filter out doubles
                    campaigns = new List<PromotionCampaign>();
                    foreach (PromotionCampaign campaign in response.Campaigns)
                    {
                        if (campaigns.All(c => c.Id != campaign.Id))
                        {
                            campaigns.Add(campaign);
                        }
                    }

                    // Cache the found result for using the DefaultRelativeExpiration
                    DistributedCache.Instance.SetObject(_CacheKey_Promotions(searchTerm), campaigns, DistributedCache.DefaultRelativeExpiration);
                }
            }

            return campaigns;
        }

        #endregion Promotions
    }
}