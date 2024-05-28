using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;

namespace nl.boxplosive.BackOffice.Mvc.Models.CreateLoyalty
{
	public class CreateLoyaltyModel : ViewModelBase, ILoyaltyModel
	{
		public StatusListModel StatusList { get; set; }

		public LoyaltyModel Loyalty { get; set; }
	}
}