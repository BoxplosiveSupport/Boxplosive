using System.ComponentModel.DataAnnotations;
using MessagePack;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageProducts
{
	[MessagePackObject(keyAsPropertyName: true)]
	public class ProductItemOverrideModel : ViewModelBase
	{
		[Required]
		public string Id { get; set; }

		[Required]
		public string Description { get; set; }
	}
}