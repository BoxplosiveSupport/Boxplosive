using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Data.SqlClient;
using nl.boxplosive.BackOffice.Business.Data;
using nl.boxplosive.BackOffice.Business.Data.Entities;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Mapping;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.ManageProducts;
using nl.boxplosive.Data.Sdk;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [boxplosive.BackOffice.Mvc.Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
    public partial class ManageProductsController : ControllerBase
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        private HttpRequest _request = System.Web.HttpContext.Current.Request;

        // GET: ManageProductsModel
        public virtual ActionResult Index()
        {
            var model = ProductFactory.CreateManageProductsModel("Manage products", PageConst.Title_NavigateToHomePage, Url.Action(MVC.Home.ActionNames.Index, MVC.Home.Name));

            var repository = DataRepositoryFactory.GetInstance().DataRepository<IProductItemOverrideRepository>();
            var records = repository.GetAllProductItemOverrides();

            model.ProductItemOverrides = records.Select(x => x.Map()).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UploadCsv()
        {
            var fileUpload = Request.Files["CsvUpload"];

            var model = new ManageProductsModel();
            var records = UploadCsv(fileUpload, model.ValidFileTypes);

            model.ProductItemOverrides = records.Select(x => x.Map()).ToList();

            return Json(model);
        }

        private IEnumerable<DtoProductItemOverride> UploadCsv(HttpPostedFileBase file, string[] fileTypes)
        {
            // Validate CSV
            if (file == null || file.ContentLength == 0)
            {
                ModelState.AddModelError("Csv", "This field is required");
            }
            else if (!fileTypes.Contains(file.ContentType))
            {
                ModelState.AddModelError("Csv", "Please choose a CSV file.");
            }

            if (!ModelState.IsValid)
            {
                return null;
            }

            List<DtoProductItemOverride> productItemOverrides = new List<DtoProductItemOverride>();
            try
            {
                // Read posted CSV
                using (var streamReader = new StreamReader(file.InputStream))
                using (var csvReader = new CsvReader(streamReader, new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ";", HasHeaderRecord = false }))
                {
                    while (csvReader.Read())
                    {
                        productItemOverrides.Add(new DtoProductItemOverride(csvReader.GetField(0), csvReader.GetField(1)));
                    }
                }

                // Save file
                var repository = DataRepositoryFactory.GetInstance().DataRepository<IProductItemOverrideRepository>();
                repository.UpdateProductItemOverrides(productItemOverrides);

                return productItemOverrides;
            }
            catch (SqlException ex)
            {
                _Logger.Warn(ex, ex.Message);
                switch (ex.Number)
                {
                    case 2627:
                        TempData["Danger"] = "Duplicates in list. List is not uploaded.";
                        break;

                    default:
                        TempData["Danger"] = "An database error occurred during file upload.";
                        break;
                }
            }
            catch (Exception ex)
            {
                _Logger.Warn(ex, ex.Message);
                TempData["Danger"] = "An error occurred during file upload.";
            }
            return productItemOverrides;
        }
    }
}