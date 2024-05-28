using System;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
	public class EdmBatchCheckPromotionModel : EdmBatchPromotionModel
	{
		public EdmBatchCheckPromotionModel()
			: base()
		{
		}

		public EdmBatchCheckPromotionModel(DtoEdmBatchPromotion edmBatchPromotionDto)
			: base(edmBatchPromotionDto)
		{
		}

		public bool IsChecked { get; set; }
	}
}