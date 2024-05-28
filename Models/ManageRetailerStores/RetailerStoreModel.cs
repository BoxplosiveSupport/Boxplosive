using nl.boxplosive.Data.Sdk;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores
{
    public class RetailerStoreModel : ViewModelBase
    {
        public RetailerStoreModel()
        {
            StoreInfo = new RetailerStoreInfoModel();
            StoreDates = new List<RetailerStoreDateModel>();
            Longitude = "0";
            Latitude = "0";
        }

        // Platform data
        public int Id { get; set; }

        public int RetailerId { get; set; }

        [Required]
        [StringLength(100)]
        [DisplayName("Name (internal)")]
        public string Name { get; set; }

        [Required]
        [RegularExpression(@"^[0-9]*$", ErrorMessage = "Must be a numerical number")]
        [StringLength(15)]
        public string Number { get; set; }

        [Required]
        [RegularExpression(@"^(-?(90|(\d|[1-8]\d)([,.]\d{1,6}){0,1}))$", ErrorMessage = "Latitude must be between -90 and 90, max 6 decimals")]
        public string Latitude { get; set; }

        [Required]
        [RegularExpression(@"^(-?(180|(\d|\d\d|1[0-7]\d)([,.]\d{1,6}){0,1}))$", ErrorMessage = "Longitude must be between -180 and 180, max 6 decimals")]
        public string Longitude { get; set; }

        public int AccountId { get; set; }

        [Required]
        public RetailerStoreType? Type { get; set; }

        // Custom data
        public RetailerStoreInfoModel StoreInfo { get; set; }

        public List<RetailerStoreDateModel> StoreDates { get; set; }
    }
}