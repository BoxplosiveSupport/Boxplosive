using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
	public class LoyaltyTabModel : BalanceTabModel
	{
		public LoyaltyTabModel()
		{
			LoyaltyPrograms = new List<SelectListItem>();
			LoyaltyMutations = new List<MutationModel>();
		}

		public int SelectedLoyaltyProgram { get; set; }

		public int TotalPoints { get; set; }

		public List<SelectListItem> LoyaltyPrograms { get; set; }

        #region Edit panel

        [DisplayName("Type")]
        public LoyaltyMutationCreditDebitType MutationCreditDebitType { get; set; } = LoyaltyMutationCreditDebitType.Credit;

        [DisplayName("Amount")]
		[Range(1, int.MaxValue, ErrorMessage = "The Amount field must be greater than or equal to 1.")]
		public int MutationAmount { get; set; }

		[MaxLength(100)]
		[Required]
		public string Comment { get; set; }

        #endregion Edit panel

        public List<MutationModel> LoyaltyMutations { get; set; }
	}
}