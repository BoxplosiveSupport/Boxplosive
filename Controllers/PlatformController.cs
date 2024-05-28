using CsvHelper;
using CsvHelper.Configuration;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.MsSql;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Service.ServiceContract.FileService.DataContracts;
using nl.boxplosive.Service.ServiceContract.ProductService.DataContracts;
using nl.boxplosive.Utilities.Date;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class PlatformController : ControllerBase
    {
        protected static readonly string ErrorMessageInvalidJsonFormat = "Invalid JSON format";

        #region Products

        private static readonly string _ProductUploadDelimiter = ";";

        [HttpPost]
        [ValidateAntiForgeryToken]
        [FeatureAttribute("CreateProduct")]
        public virtual ActionResult AddProduct(string gtin)
        {
            Service.ServiceModel.Product product = new Service.ServiceModel.Product();

            List<Service.ServiceModel.Product> products = PlatformHelper.GetProductsByGtin(new List<string> { gtin }).ToList();
            if (products.Count == 0)
            {
                Product businessProduct =
                    ProductApi.AddProduct(new Product(new DtoProduct(gtin, string.Empty)));
                product = new Service.ServiceModel.Product()
                {
                    Id = businessProduct.Id,
                    Name = businessProduct.Name,
                    Description = businessProduct.Description
                };
            }
            else
            {
                product.Id = "error";
                product.Name = "error";
                product.Description = "Duplicate product";
            }

            return Json(product, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetProducts(string term, string parentId)
        {
            var products = PlatformHelper.GetProducts(term, parentId);

            var list = new List<object>();

            foreach (var product in products)
            {
                list.Add(new
                {
                    id = product.Id,
                    label = product.ToString(),
                    value = product.ToString()
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet);
        }

        public virtual ActionResult GetGroups(string term)
        {
            var dbProductGroups = PlatformHelper.GetProductGroups(term);

            var productGroups = new List<object>();
            foreach (var dbProductGroup in dbProductGroups)
            {
                if (!string.IsNullOrEmpty(dbProductGroup.Name))
                    productGroups.Add(new
                    {
                        id = dbProductGroup.Name,
                        label = dbProductGroup.Name,
                        value = dbProductGroup.Name,
                        sublabel = dbProductGroup.Name,
                        folder = true
                    });
            }

            // Magic product group: contains all products
            productGroups.Add(new
            {
                id = string.Empty,
                label = "All",
                value = "All",
                sublabel = "All",
                folder = true
            });

            return Json(productGroups, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ValidateAndSetMultiProducts(string source)
        {
            HttpPostedFileBase httpPostedFileBase = Request.Files.Get(0);

            var uploadedProducts = new List<UploadedProduct>();
            using (var streamReader = new StreamReader(httpPostedFileBase.InputStream))
            {
                using (var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = _ProductUploadDelimiter, HasHeaderRecord = false }))
                {
                    while (csvReader.Read())
                    {
                        if (!csvReader.TryGetField(1, out string name))
                        {
                            name = "";
                        }

                        uploadedProducts.Add(new UploadedProduct() { Gtin = csvReader.GetField(0).Trim(), Name = name });
                    }
                }
            }

            var request = new GetProductsWithValidationRequest
            {
                UploadedProducts = uploadedProducts
            };
            AuthenticationHelpers.SetupServiceRequest(request, "GetProductsWithValidation");
            GetProductsWithValidationResponse response = ServiceCallFactory.Product_GetProductsWithValidation(request);

            // Note: that JSON content length should be really compact (by default the limit is 4 MB)
            // @see https://msdn.microsoft.com/en-us/library/system.web.script.serialization.javascriptserializer.maxjsonlength.aspx
            var selected = new List<object[]>();
            var errors = new List<object[]>();
            var newItems = 0;
            foreach (ProductValidation productValidation in response.ProductValidations)
            {
                // Errors
                bool error = false;
                string errorType = null;
                if (productValidation.Occurrences > 1)
                {
                    error = true;
                    errorType = "D";
                    int duplicateCount = productValidation.Occurrences - 1;
                    errors.Add(new object[3]
                    {
                        productValidation.UploadedProduct.Gtin,
                        errorType,
                        duplicateCount,
                    });
                }

                if (!productValidation.ProductExists)
                {
                    newItems++;

                    if (!AppConfig.Settings.Feature_CreateProduct)
                    {
                        error = true;
                        errorType = "U";
                        errors.Add(new object[3]
                        {
                            productValidation.UploadedProduct.Gtin,
                            errorType,
                            productValidation.Occurrences
                        });
                    }
                }

                if (error)
                    continue;

                // Add to selection
                selected.Add(new object[2]
                {
                    productValidation.UploadedProduct.Gtin,
                    productValidation.UploadedProduct.Name,
                });
            }

            var data = new
            {
                MultiItemFileName = httpPostedFileBase.FileName,
                MultiItemCount = uploadedProducts.Count,
                // Only set selected when there are no validation errors
                MultiItemSelected = errors.Any() ? new List<object[]>() : selected,
                MultiItemNewCount = newItems,
                MultiItemErrorCount = errors.Count,
                MultiItemErrors = errors,
                MultiItemDisplayThreshold = AppConfig.Settings.ProductListThreshold,
            };

            string key = GetProductUploadKey(source, isTemp: true);
            TempData[key] = errors.Count == 0 ? response.ProductValidations : new HashSet<ProductValidation>();

            // Create a new serializer to specifiy the MaxJsonLength for returning large data sets
            // https://stackoverflow.com/questions/1151987/can-i-set-an-unlimited-length-for-maxjsonlength-in-web-config/7207539#7207539
            var serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var result = new ContentResult
            {
                Content = serializer.Serialize(data),
                ContentType = "application/json"
            };

            return result;
        }

        public static string GetProductUploadKey(string source, bool isTemp = false)
        {
            return string.Format("MultipleProducts{0}Upload{1}",
                    isTemp ? "Temp" : "",
                    string.IsNullOrEmpty(source) ? "" : "_" + source
                );
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpPost]
        public virtual JsonResult ConfirmProductUpload(string source)
        {
            var apiClientId = AppConfig.Settings.ApiClientId;
            IDtoApiClient dtoClient = new ApiClientRepository().Get(apiClientId);
            LabelInternalApi.Load();

            string tempKey = GetProductUploadKey(source, isTemp: true);
            string key = GetProductUploadKey(source);

            var products = (HashSet<ProductValidation>)TempData[tempKey];
            if (products != null)
            {
                foreach (var product in products)
                {
                    if (!product.ProductExists)
                    {
                        ProductApi.AddProduct(new Product(new DtoProduct(product.UploadedProduct.Gtin, product.UploadedProduct.Name)));
                    }
                    // Within CSV upload, the name is optional, and lines of the form of both 'gtin;' and 'gtin' should not update the name
                    else if (!string.IsNullOrWhiteSpace(product?.UploadedProduct?.Name) && product.UploadedProduct.Name != product.KnownProduct.Name)
                    {
                        product.KnownProduct.Name = product.UploadedProduct.Name;
                        ProductApi.UpdateProduct(product.KnownProduct);
                    }
                }

                TempData.Remove(tempKey);
                TempData[key] = products;
                return Json(products.Count);
            }

            return Json(0);
        }

        protected void LoadProductsFromMultipleProductUpload(List<ProductModel> modelProducts, List<ProductModel> cachedProducts, string source)
        {
            var key = PlatformController.GetProductUploadKey(source);

            var productValidations = (HashSet<ProductValidation>)TempData[key];

            if (productValidations != null)
            {
                foreach (var productValidation in productValidations)
                {
                    if (!modelProducts.Any(mp => mp.Id == productValidation.UploadedProduct.Gtin))
                    {
                        modelProducts.Add(new ProductModel { Id = productValidation.UploadedProduct.Gtin, Name = productValidation.UploadedProduct.Name });
                    }
                }
                TempData[key] = null;
            }
            else
            {
                var missingItems = cachedProducts.Where(cachedItem => !modelProducts.Exists(mi => mi.Id == cachedItem.Id)).ToList();
                modelProducts.AddRange(missingItems);
            }
        }

        public virtual FileContentResult GetDownloadProductsFileResult(IList<ProductModel> products, string productSetLabel)
        {
            return File(
                products.SelectMany(p => Encoding.UTF8.GetBytes(p.Id + Environment.NewLine)).ToArray(),
                "text/csv",
                String.Format("{0}-{1}.csv", productSetLabel, OperatingDateTime.NowLocal.ToString("yyyyMMdd-hhmm"))
            );
        }

        protected List<ProductModel> GetProducts(ProductsType productsType, IProductListsModel productListsModel)
        {
            if (productListsModel == null)
            {
                return new List<ProductModel>();
            }

            switch (productsType)
            {
                case ProductsType.Triggers:
                    return productListsModel.Triggers;

                case ProductsType.SelectedProducts:
                    return productListsModel.SelectedProducts;

                case ProductsType.TargetProducts:
                    return productListsModel.TargetProducts;

                default:
                    return new List<ProductModel>();
            }
        }

        #endregion Products

        #region Promotions

        public virtual ActionResult GetPromotions(string term)
        {
            var promotions = PlatformHelper.GetPromotions(term);

            var list = new List<object>();

            foreach (var promotion in promotions)
            {
                list.Add(new
                {
                    id = promotion.Id,
                    label = promotion.Title,
                    value = promotion.Title
                });
            }

            if (!list.Any())
            {
                list.Add(new
                {
                    id = "",
                    label = "No promotions found",
                    value = "No promotions found"
                });
            }

            return Json(list, JsonRequestBehavior.AllowGet); ;
        }

        #endregion Promotions

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UploadImage(string postedFileFieldName, string urlFieldName, string urlFieldId)
        {
            ImageHelper.ValidateAndUpload(out SaveFileResponse uploadResult, Request.Files[postedFileFieldName], ModelState, urlFieldName);

            return Json(new Dictionary<string, object> { { urlFieldId, uploadResult?.Url } });
        }

        [HttpPost]
        public virtual ActionResult UploadImageCKEditor(HttpPostedFileBase upload, string ckCsrfToken)
        {
            try
            {
                ManuallyValidateAntiForgeryToken();
            }
            catch (Exception _)
            {
                return Json(new { error = new { message = "A required anti-forgery token was not supplied or was invalid." } });
            }

            if (!ImageHelper.Validate(upload, out string errorMessage))
                return Json(new { uploaded = 0, error = new { message = errorMessage } });

            SaveFileResponse uploadResult = ImageHelper.Upload(upload);

            return Json(new { url = uploadResult.Url });
        }

        /// <remarks>
        /// https://learn.microsoft.com/en-us/aspnet/web-api/overview/security/preventing-cross-site-request-forgery-csrf-attacks#anti-csrf-and-ajax
        /// </remarks>
        private void ManuallyValidateAntiForgeryToken()
        {
            string cookieToken = "";
            string formToken = "";

            string headerKey = Request.Headers.AllKeys.SingleOrDefault(key => key.Equals("RequestVerificationToken", StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(headerKey))
            {
                string[] tokens = Request.Headers.GetValues(headerKey).First().Split(':');
                if (tokens.Length == 2)
                {
                    cookieToken = tokens[0].Trim();
                    formToken = tokens[1].Trim();
                }
            }

            AntiForgery.Validate(cookieToken, formToken);
        }
    }
}
