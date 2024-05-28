using nl.boxplosive.BackOffice.Business.Entities;
using nl.boxplosive.BackOffice.Mvc.Mapping;
using nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores;
using nl.boxplosive.Sdk.Exceptions;
using PagedList;
using System;
using System.Collections.Generic;
using Store = nl.boxplosive.Business.Sdk.Entities.Store;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class RetailerStoreFactory : ViewModelBaseFactory<RetailerStoreFactory>
    {
        public static ManageRetailerStoresModel CreateManageRetailerStoresModel(string pageTitle,
            string pageNavigationLabel, string pageNavigationUrl,
            int? pageNr, string searchText, List<RetailerStore> stores)
        {
            #region Preconditions

            if (stores == null)
            {
                throw new BoxplosiveArgumentNullException("stores");
            }

            #endregion Preconditions

            var model = Instance.CreateModel<ManageRetailerStoresModel>(pageTitle, pageNavigationLabel,
                pageNavigationUrl);

            model.SearchText = searchText;

            var pageNumber = pageNr ?? 1;

            var storelist = new List<RetailerStoreModel>();
            foreach (var store in stores)
            {
                var storeModel = store.Store.Map();
                storeModel.StoreInfo = store.StoreInfo.Map();
                storelist.Add(storeModel);
            }
            model.RetailerStores = storelist.ToPagedList(pageNumber, 10);

            return model;
        }

        public static RetailerStoreModel CreateRetailerStoreModel(string pageTitle, string pageNavigationLabel,
            string pageNavigationUrl, int retailerId, RetailerStoreModel store = null)
        {
            var model = store == null ?
                Instance.CreateModel<RetailerStoreModel>(pageTitle, pageNavigationLabel, pageNavigationUrl) :
                Instance.UpdateModel(pageTitle, pageNavigationLabel, pageNavigationUrl, store);

            if (store == null)
            {
                model.RetailerId = retailerId;
            }

            if (model.StoreInfo == null)
            {
                model.StoreInfo = new RetailerStoreInfoModel();
            }

            model.StoreInfo.RetailerId = retailerId;
            model.StoreInfo.RetailerStoreId = model.Id;

            if (model.StoreDates == null)
            {
                model.StoreDates = new List<RetailerStoreDateModel>();
            }

            if (model.StoreDates.Count == 0)
            {
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Monday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Tuesday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Wednesday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Thursday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Friday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Saturday
                });
                model.StoreDates.Add(new RetailerStoreDateModel
                {
                    KeepDefault = true,
                    Day = WeekDay.Sunday
                });
            }

            // Set the correct RetailerId for each date
            model.StoreDates.ForEach(sd => sd.RetailerId = retailerId);
            model.StoreDates.ForEach(sd => sd.RetailerStoreId = model.Id);

            return model;
        }

        public static RetailerStore ConvertRetailerStoreModelToEntity(RetailerStoreModel model)
        {
            // Clean up Date list
            model.StoreDates.RemoveAll(sd => sd.Remove && sd.Id <= 0);

            var retailerStore = new RetailerStore();
            retailerStore.Store = new Store(model.Map());
            retailerStore.StoreInfo = model.StoreInfo.Map();
            retailerStore.StoreDates = model.StoreDates.Map();

            // Clean up data
            foreach (var date in retailerStore.StoreDates)
            {
                if (date.KeepDefault)
                {
                    date.Closed = false;
                    date.Date = null;
                    date.TimeFrom = new TimeSpan();
                    date.TimeTill = new TimeSpan();
                }

                if (date.Closed)
                {
                    date.TimeFrom = new TimeSpan();
                    date.TimeTill = new TimeSpan();
                }

                date.RetailerId = retailerStore.StoreInfo.RetailerId;
                date.RetailerStoreId = retailerStore.StoreInfo.RetailerStoreId;
            }

            return retailerStore;
        }

        public static RetailerStoreModel ConvertRetailerStoreEntityToModel(RetailerStore retailerStore)
        {
            var model = retailerStore?.Store?.Map() ?? new RetailerStoreModel();
            model.StoreInfo = retailerStore?.StoreInfo?.Map() ?? new RetailerStoreInfoModel();
            model.StoreDates = retailerStore?.StoreDates?.Map() ?? new List<RetailerStoreDateModel>();

            var retailer = retailerStore?.Store?.Retailer;
            if (retailer != null)
                model.RetailerId = retailer.Id;

            var account = retailerStore?.Store?.Account;
            if (account != null)
                model.AccountId = account.Id;

            return model;
        }
    }
}