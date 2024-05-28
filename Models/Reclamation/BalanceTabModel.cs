using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
	public class BalanceTabModel : TabModelBase
	{
		public bool HasBalanceInfo { get; private set; }

		public List<string> BalanceInfo
		{
			get
			{
				return _BalanceInfo;
			}
		}

		private List<string> _BalanceInfo = new List<string>();

		public void SetBalanceInfo(bool subscribed, bool openOrMember)
		{
			_BalanceInfo = new List<string>();
			if (!subscribed)
			{
				_BalanceInfo.Add("The customer is not subscribed");
				HasBalanceInfo = true;
			}

			if (!openOrMember)
			{
				HasBalanceInfo = true;
				_BalanceInfo.Add("The customer is not a member");
			}
		}
	}
}