using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;

namespace nl.boxplosive.BackOffice.Mvc.Models.CreatePromotion
{
	public class CreatePromotionModel : ViewModelBase, IPromotionModel
    {
        public StatusListModel StatusList { get; set; }

        public PromotionModel Promotion { get; set; }
    }
}