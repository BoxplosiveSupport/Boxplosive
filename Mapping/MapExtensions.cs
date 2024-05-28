using nl.boxplosive.BackOffice.Mvc.Models.ManageProducts;
using nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores;
using nl.boxplosive.BackOffice.Mvc.Models.ManageVersionMessage;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.BackOffice.Business.Business.StartupMessage;
using nl.boxplosive.BackOffice.Business.Entities;
using nl.boxplosive.BackOffice.Business.Data.Entities;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ProcessModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Mapping
{
    public static class MapExtensions
    {
        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">CampaignProcessBase</param>
        /// <param name="to">PublishCampaignProcess</param>
        /// <returns>PublishCampaignProcess</returns>
        public static CampaignProcess Map(this CampaignProcess from, CampaignProcess to = null)
        {
            if (to == null)
            {
                to = new CampaignProcess();
            }

            to.State = from.State;
            to.Campaign = from.Campaign;
            to.Tag = from.Tag;
            to.Id = from.Id;
            to.CreatedByIssuerId = from.CreatedByIssuerId;
            to.CreatedDate = from.CreatedDate;
            to.ModifiedByIssuerId = from.ModifiedByIssuerId;
            to.ModifiedDate = from.ModifiedDate;
            to.RowVersion = from.RowVersion;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">DtoProductItemOverride</param>
        /// <param name="to">ProductItemOverrideModel</param>
        /// <returns>ProductItemOverrideModel</returns>
        public static ProductItemOverrideModel Map(this DtoProductItemOverride from, ProductItemOverrideModel to = null)
        {
            if (to == null) to = new ProductItemOverrideModel();

            to.Id = from.Id;
            to.Description = from.Description;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">Store</param>
        /// <param name="to">RetailerStoreModel</param>
        /// <returns>RetailerStoreModel</returns>
        public static RetailerStoreModel Map(this boxplosive.Business.Sdk.Entities.Store from, RetailerStoreModel to = null)
        {
            if (from == null) return null;
            if (to == null) to = new RetailerStoreModel();

            to.Id = from.Id;
            to.Name = from.Name;
            to.Number = from.Number;
            to.Latitude = from.Latitude.ToString();
            to.Longitude = from.Longitude.ToString();
            to.Type = Enum.TryParse(from.Type, ignoreCase: true, out RetailerStoreType storeType) ? storeType : (RetailerStoreType?)null;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">RetailerStoreInfo</param>
        /// <param name="to">RetailerStoreInfoModel</param>
        /// <returns>RetailerStoreInfoModel</returns>
        public static RetailerStoreInfoModel Map(this RetailerStoreInfo from, RetailerStoreInfoModel to = null)
        {
            if (to == null) to = new RetailerStoreInfoModel();

            to.Id = from.Id;
            to.RetailerStoreId = from.RetailerStoreId;
            to.RetailerId = from.RetailerId;
            to.StoreName = from.StoreName;
            to.AddressLine1 = from.AddressLine1;
            to.AddressLine2 = from.AddressLine2;
            to.Zipcode = from.Zipcode;
            to.City = from.City;
            to.Country = from.Country;
            to.TelephoneNumber = from.TelephoneNumber;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">RetailerStoreModel</param>
        /// <param name="to">DtoStore</param>
        /// <returns>DtoStore</returns>
        public static DtoStore Map(this RetailerStoreModel from, DtoStore to = null)
        {
            if (to == null) to = new DtoStore();

            to.Id = from.Id;
            to.RetailerId = from.RetailerId;
            to.Name = from.Name;
            to.Number = from.Number;
            to.Latitude = Convert.ToDouble(from.Latitude.Replace(",", "."), CultureInfo.InvariantCulture);
            to.Longitude = Convert.ToDouble(from.Longitude.Replace(",", "."), CultureInfo.InvariantCulture);
            to.AccountId = from.AccountId;
            to.Type = from.Type.ToString();

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">RetailerStoreInfoModel</param>
        /// <param name="to">RetailerStoreInfo</param>
        /// <returns>RetailerStoreInfo</returns>
        public static RetailerStoreInfo Map(this RetailerStoreInfoModel from, RetailerStoreInfo to = null)
        {
            if (to == null) to = new RetailerStoreInfo();

            to.Id = from.Id;
            to.RetailerStoreId = from.RetailerStoreId;
            to.RetailerId = from.RetailerId;
            to.StoreName = from.StoreName;
            to.AddressLine1 = from.AddressLine1;
            to.AddressLine2 = from.AddressLine2;
            to.Zipcode = from.Zipcode;
            to.City = from.City;
            to.Country = from.Country;
            to.TelephoneNumber = from.TelephoneNumber;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">RetailerStoreDateModel</param>
        /// <param name="to">RetailerStoreDate</param>
        /// <returns>RetailerStoreDate</returns>
        public static RetailerStoreDate Map(this RetailerStoreDateModel from, RetailerStoreDate to = null)
        {
            if (to == null) to = new RetailerStoreDate();

            to.Id = from.Id;
            to.RetailerStoreId = from.RetailerStoreId;
            to.RetailerId = from.RetailerId;
            to.KeepDefault = from.KeepDefault;
            to.Day = (int?)from.Day;
            to.Date = from.Date;
            to.Closed = from.Closed;
            to.TimeFrom = from.TimeFrom;
            to.TimeTill = from.TimeTill;
            to.Remove = from.Remove;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">List|RetailerStoreDateModel</param>
        /// <param name="to">List|RetailerStoreDate</param>
        /// <returns>List|RetailerStoreDate</returns>
        public static List<RetailerStoreDate> Map(this List<RetailerStoreDateModel> from, List<RetailerStoreDate> to = null)
        {
            if (to == null) to = new List<RetailerStoreDate>();
            to.AddRange(@from.Select(fromItem => fromItem.Map()));
            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">RetailerStoreDate</param>
        /// <param name="to">RetailerStoreDateModel</param>
        /// <returns>RetailerStoreDateModel</returns>
        public static RetailerStoreDateModel Map(this RetailerStoreDate from, RetailerStoreDateModel to = null)
        {
            if (to == null) to = new RetailerStoreDateModel();

            to.Id = from.Id;
            to.RetailerStoreId = from.RetailerStoreId;
            to.RetailerId = from.RetailerId;
            to.KeepDefault = from.KeepDefault;
            to.Day = (WeekDay?)from.Day;
            to.Date = from.Date;
            to.Closed = from.Closed;
            to.TimeFrom = from.TimeFrom;
            to.TimeTill = from.TimeTill;
            to.Remove = from.Remove;

            return to;
        }

        /// <summary>
        /// Map data from one instance to another.
        /// </summary>
        /// <param name="from">List|RetailerStoreDate</param>
        /// <param name="to">List|RetailerStoreDateModel</param>
        /// <returns>List|RetailerStoreDateModel</returns>
        public static List<RetailerStoreDateModel> Map(this List<RetailerStoreDate> from, List<RetailerStoreDateModel> to = null)
        {
            if (to == null) to = new List<RetailerStoreDateModel>();
            to.AddRange(@from.Select(fromItem => fromItem.Map()));
            return to;
        }

        public static VersionMessageModel Map(this VersionMessage from, VersionMessageModel to = null)
        {
            to = to ?? new VersionMessageModel();

            to.AppVersion = from.AppVersion;
            to.AlertMessage = from.AlertMessage;
            to.ErrorMessage = from.ErrorMessage;
            to.New = false;

            return to;
        }

        public static VersionMessage Map(this VersionMessageModel from, VersionMessage to = null)
        {
            to = to ?? new VersionMessage();

            to.AppVersion = from.AppVersion;
            to.AlertMessage = DataHelper.Sanitize(from.AlertMessage);
            to.ErrorMessage = DataHelper.Sanitize(from.ErrorMessage);

            return to;
        }
    }
}