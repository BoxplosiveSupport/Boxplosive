using System.Collections.Generic;
using System.Security.Principal;
using System.Web;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Configuration;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
	public static class ProcessModuleExtensions
	{
		public static void CheckFeatureAndAdd(this List<ProcessModule> modules, ProcessModule processModule)
		{
			if (!string.IsNullOrEmpty(processModule.FeatureName))
			{
				bool isEnabled = AppConfig.Settings.IsFeatureEnabled(processModule.FeatureName);
				if (!isEnabled)
				{
					return;
				}
			}

			IPrincipal user;
			if (processModule.Roles != null && processModule.Roles.Count > 0)
			{
				bool isInOneRole = HttpContext.Current.User.IsInOneRole(processModule.Roles);
				if (!isInOneRole)
				{
					return;
				}
			}

			modules.Add(processModule);
		}
	}
}