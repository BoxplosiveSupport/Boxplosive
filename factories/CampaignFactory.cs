using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
	public class CampaignFactory
	{
		public static CampaignProcess GetCampaignProcess(int id)
		{
			var request = new GetCampaignProcessRequest
			{
				Id = id
			};

			AuthenticationHelpers.SetupServiceRequest(request, "Get");
			var response = ServiceCallFactory.CampaignProcess_Get(request);
			return response.Process;
		}

		public static List<ProductModel> CreateProductModelList(HashSet<string> productIds)
		{
			var productModels = new List<ProductModel>();

			var productEnumerable = PlatformHelper.GetProductsByGtin(productIds) ?? new List<Product>();
			var products = productEnumerable.ToDictionary(p => p.Id);

			if (productIds.Count < AppConfig.Settings.ProductListThreshold)
			{
				foreach (var productId in productIds)
				{
					Product product = null;
					var productModel = new ProductModel
					{
						Id = productId,
						Name = products.TryGetValue(productId, out product) ? product.ToString() : "[unknown product id: " + productId + "]"
					};

					productModels.Add(productModel);
				}
			}
			else
			{
				foreach (var productId in productIds)
				{
					var productModel = new ProductModel
					{
						Id = productId,
						Name = string.Empty
					};

					productModels.Add(productModel);
				}
			}


			return productModels;
		}
	}
}