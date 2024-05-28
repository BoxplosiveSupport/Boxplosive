using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;

namespace nl.boxplosive.BackOffice.Mvc.Models.ModifyPromotion
{
	public class ModifyPromotionModel : ViewModelBase, IPromotionModel
	{
		public StatusListModel StatusList { get; set; }

		public string PromotionCode { get; set; }

		public PromotionModel Promotion { get; set; }
	}
}