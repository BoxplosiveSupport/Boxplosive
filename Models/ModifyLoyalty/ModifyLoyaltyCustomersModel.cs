using System.ComponentModel.DataAnnotations;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models.ModifyLoyalty
{
	public class ModifyLoyaltyCustomersModel
	{
		public ModifyLoyaltyCustomersModel()
		{
		}

		public ModifyLoyaltyCustomersModel(IDtoLoyaltyPointsProgram loyalty)
		{
			Id = loyalty.Id;
			Name = loyalty.Name;
			IsPublic = loyalty.IsPublic;
			IsAutomaticallySubscribed = loyalty.IsAutomaticallySubscribed;
		}

		public int Id { get; set; }

		public string Name { get; set; }

		[Display(Name = "Open to all customers")]
		public bool IsPublic { get; set; }

		[Display(Name = "Automatically subscribe")]
		public bool IsAutomaticallySubscribed { get; set; }

		[Display(Name = "Selected customers")]
		public int EnabledBalancesCount { get; set; }
	}
}