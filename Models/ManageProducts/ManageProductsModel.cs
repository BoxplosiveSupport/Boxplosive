using Foolproof;
using MessagePack;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageProducts
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ManageProductsModel : ViewModelBase
    {
        public List<ProductItemOverrideModel> ProductItemOverrides { get; set; }

        [DisplayName("Csv Upload")]
        public int CsvUrl { get; set; }

        public string[] ValidFileTypes =
            {
                "text/csv",
                "text/plain",
                "application/csv",
                "text/comma-separated-values",
                "application/excel",
                "application/vnd.ms-excel",
                "application/vnd.msexcel",
                "text/anytext",
                "application/octet-stream",
                "application/txt"
            };

        [DisplayName("Csv Upload")]
        [IgnoreMember]
        [RequiredIfEmpty("CsvUrl", ErrorMessage = "An Csv is required")]
        public HttpPostedFileBase CsvUpload { get; set; }
    }
}