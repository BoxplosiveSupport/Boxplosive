using System;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
	public class EdmBatchModel
	{
		public EdmBatchModel()
		{
		}

		public EdmBatchModel(IDtoEdmBatch edmBatchDto)
		{
			Id = edmBatchDto.Id;
			CreatedAt = edmBatchDto.CreatedDate;
			FileName = edmBatchDto.File;
			PromotionCount = edmBatchDto.PromotionCount;
			LineCount = edmBatchDto.LineCount;
			HasCancelReservationsAction = GetHasCancelReservationsActionValue(edmBatchDto);
		}

		[Display(Name = "Batch")]
		public int Id { get; set; }

		[DataType(DataType.Date)]
		[Display(Name = "Date")]
		[DisplayFormat(DataFormatString = "{0:d}", ApplyFormatInEditMode = true)]
		public DateTime CreatedAt { get; set; }

		[Display(Name = "Filename")]
		public string FileName { get; set; }

		[Display(Name = "Promotions")]
		public int PromotionCount { get; set; }

		[Display(Name = "Lines")]
		public int LineCount { get; set; }

		public bool HasCancelReservationsAction { get; set; }

		private bool GetHasCancelReservationsActionValue(IDtoEdmBatch edmBatchDto)
		{
			return edmBatchDto.Format == EdmBatchFormat.Regular && edmBatchDto.PromotionCount > 0;
		}
	}
}