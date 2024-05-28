using System;
using System.Web;
using System.Web.Mvc;
using nl.boxplosive.Configuration;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
	public class FeatureAttribute : FilterAttribute
	{
		public FeatureAttribute(string featureName, bool expectedValue = true)
		{
			if (AppConfig.Settings.IsFeatureEnabled(featureName) != expectedValue)
			{
				throw new HttpException(404, "Not Found");
			}
		}
	}
}