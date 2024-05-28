using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Enums
{
	[Flags]
	public enum DayRuleChecks
	{
		None = 0,      // 0000000
		Sunday = 1,    // 0000001
		Monday = 2,    // 0000010
		Tuesday = 4,   // 0000100
		Wednesday = 8, // 0001000
		Thursday = 16, // 0010000
		Friday = 32,   // 0100000
		Saturday = 64, // 1000000
		WorkDay = Monday | Tuesday | Wednesday | Thursday | Friday,
		Weekend = Sunday | Saturday
	}
}