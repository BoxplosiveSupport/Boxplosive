using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.ModifyLoyalty
{
	public class ModifyLoyaltyGrantLoyaltyPointsModel
	{
		public ModifyLoyaltyGrantLoyaltyPointsModel()
		{
			NumberOfLoyaltyPoints = 1;
		}

		public ModifyLoyaltyGrantLoyaltyPointsModel(IDtoLoyaltyPointsProgram loyalty)
			: this()
		{
			Id = loyalty.Id;
			Name = loyalty.Name;
		}

		public int Id { get; set; }

		public string Name { get; set; }

		[Display(Name = "Number of loyalty points")]
		[Range(1, int.MaxValue, ErrorMessage = "The number of loyalty points should be greater than or equal to 1")]
		public int NumberOfLoyaltyPoints { get; set; }

		[Display(Name = "Selected customers")]
		public int SelectedCustomers { get; set; }
	}
}