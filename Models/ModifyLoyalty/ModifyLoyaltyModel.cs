using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;

namespace nl.boxplosive.BackOffice.Mvc.Models.ModifyLoyalty
{
	public class ModifyLoyaltyModel : ViewModelBase, ILoyaltyModel
	{
		public ModifyStatusListModel StatusList { get; set; }

		public int LoyaltyProgramId { get; set; }

		public LoyaltyModel Loyalty { get; set; }
	}
}