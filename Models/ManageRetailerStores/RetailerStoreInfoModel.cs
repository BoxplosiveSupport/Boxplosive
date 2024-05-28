using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores
{
	public class RetailerStoreInfoModel
	{
		public int Id { get; set; }
		public int RetailerStoreId { get; set; }
		public int RetailerId { get; set; }
		[DisplayName("Store Name")]
		[StringLength(255)]
        public string StoreName { get; set; }
		[StringLength(255)]
        public string AddressLine1 { get; set; }
		[StringLength(255)]
        public string AddressLine2 { get; set; }
		[StringLength(15)]
		public string Zipcode { get; set; }
		[StringLength(255)]
        public string City { get; set; }
		[StringLength(255)]
        public string Country { get; set; }
		[DisplayName("Telephone number")]
		[RegularExpression(@"^(((0)[1-9]{2}[0-9][-]?[1-9][0-9]{5})|((\\+31|0|0031)[1-9][0-9][-]?[1-9][0-9]{6}))$", ErrorMessage = "Entered phone format is not valid.")]
		[StringLength(15)]
        public string TelephoneNumber { get; set; }
	}
}