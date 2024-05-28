using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
	[Flags]
	public enum LoyaltySteps
	{
		Basics = 1,
		Timeframe = 2,
		Rewards = 4,
		Savings = 8
	}
}