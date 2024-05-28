using System;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
	public class EdmBatchPromotionModel
	{
		public EdmBatchPromotionModel()
		{
		}

		public EdmBatchPromotionModel(DtoEdmBatchPromotion edmBatchPromotionDto)
		{
			Id = edmBatchPromotionDto.CampaignId;
			Title= edmBatchPromotionDto.CampaignTitle;
			InactiveReservationCount= edmBatchPromotionDto.InactiveReservationCount;
		}

		[Display(Name = "Promotion")]
		public string Id { get; set; }

		[Display(Name = "Title")]
		public string Title { get; set; }

		[Display(Name = "Inactive reservations")]
		public int InactiveReservationCount { get; set; }
	}
}