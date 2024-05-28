using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores;
using nl.boxplosive.Configuration;
using System;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [boxplosive.BackOffice.Mvc.Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class ManageRetailerStoresController : nl.boxplosive.BackOffice.Mvc.Controllers.ControllerBase
    {
        public virtual ActionResult Index(int? page, string searchText)
        {
            var loginName = AppConfig.Settings.RetailerLoginName;
            var stores = BackOfficeBusinessCallFactory.Retailer_GetRetailerStores(loginName);

            if (!string.IsNullOrEmpty(searchText))
            {
                stores = stores.Where(s =>
                    s.Store.Name.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (s.StoreInfo.StoreName != null && s.StoreInfo.StoreName.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0) ||
                    s.Store.Number.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            var model = RetailerStoreFactory.CreateManageRetailerStoresModel(
                "Manage retailer stores",
                PageConst.Title_NavigateToHomePage,
                Url.Action(MVC.Home.ActionNames.Index, MVC.Home.Name),
                page,
                searchText,
                stores
            );

            return View(model);
        }

        // Id = RetailerStore.Id (RetailerStoreID in DB)
        public virtual ActionResult Edit(int id = 0)
        {
            RetailerStoreModel model = null;
            int retailerId = AppConfig.Settings.RetailerId;
            if (id > 0)
            {
                var retailerStore = BackOfficeBusinessCallFactory.Retailer_GetRetailerStore(retailerId, id);
                model = RetailerStoreFactory.ConvertRetailerStoreEntityToModel(retailerStore);
            }

            model = RetailerStoreFactory.CreateRetailerStoreModel("Manage retailer store", PageConst.Title_NavigateToPreviousPage,
                    Url.Action("Index"), retailerId, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(RetailerStoreModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var newStore = model.Id == 0;

            var password = AppConfig.Settings.RetailerStoreDefaultPassword ?? "NewPassword";
            var storeId = BackOfficeBusinessCallFactory.Retailer_SaveRetailerStore(RetailerStoreFactory.ConvertRetailerStoreModelToEntity(model), password);

            if (storeId == -1)
            {
                ModelState.AddModelError("StoreNumber", "StoreNumber already exists in the database.");
                TempData["Danger"] = "StoreNumber already exists in the database.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public virtual ActionResult Delete(int id)
        {
            RetailerStoreModel model = null;
            int retailerId = AppConfig.Settings.RetailerId;
            if (id > 0)
            {
                var retailerStore = BackOfficeBusinessCallFactory.Retailer_GetRetailerStore(retailerId, id);
                model = RetailerStoreFactory.ConvertRetailerStoreEntityToModel(retailerStore);
            }

            model = RetailerStoreFactory.CreateRetailerStoreModel("Delete retailer store", PageConst.Title_NavigateToPreviousPage,
                    Url.Action("Index"), retailerId, model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(RetailerStoreModel model)
        {
            var store = BackOfficeBusinessCallFactory.Retailer_GetRetailerStore(model.RetailerId, model.Id);
            BackOfficeBusinessCallFactory.Retailer_DeleteRetailerStore(store);

            return RedirectToAction(ActionNames.Index);
        }
    }
}