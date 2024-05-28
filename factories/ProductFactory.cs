using System.Collections.Generic;
using nl.boxplosive.BackOffice.Mvc.Models.ManageProducts;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Service.ServiceModel;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
	public class ProductFactory : ViewModelBaseFactory<ProductFactory>
	{
		public static ManageProductsModel CreateManageProductsModel(string pageTitle, string pageNavigationLabel, string pageNavigationUrl)
		{
			var model = Instance.CreateModel<ManageProductsModel>(pageTitle, pageNavigationLabel, pageNavigationUrl);
			model.ProductItemOverrides = new List<ProductItemOverrideModel>();

			return model;
		}

		/// <summary>
		/// Add and/or remove products.
		/// </summary>
		/// <param name="products">The products to add or remove.</param>
		/// <param name="uploadProducts">The products from the upload file.</param>
		/// <returns>The processed products.</returns>
		public static HashSet<string> ProcessProducts(List<ProductModel> products, List<string> uploadProducts)
		{
			var productIds = new HashSet<string>();
			// Process selected products if there are any
			if (products != null && products.Count > 0)
			{
				foreach (var product in products)
				{
					// Set the products, hashset auto filters duplicates
					if (!product.Remove)
					{
						productIds.Add(product.Id);
					}
				}

				products.RemoveAll(sp => sp.Remove);
			}

			// Add the uploaded selected products to the selected products
			if (uploadProducts != null && uploadProducts.Count > 0)
			{
				productIds = productIds ?? new HashSet<string>();

				foreach (var uploadProduct in uploadProducts)
				{
					productIds.Add(uploadProduct);
				}
			}

			return productIds;
		}
	}
}