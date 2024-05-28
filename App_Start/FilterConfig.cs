using System.Web.Mvc;
using System.Web.UI;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using AuthorizeAttribute = nl.boxplosive.BackOffice.Mvc.Attributes.AuthorizeAttribute;

namespace nl.boxplosive.BackOffice.Mvc
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new OutputCacheAttribute
			{
				VaryByParam = "*",
				Duration = 0,
				NoStore = true,
				Location = OutputCacheLocation.None
			});
            filters.Add(new AiHandleErrorAttribute());
            filters.Add(new RequireHttpsAttribute());
			filters.Add(new AuthorizeAttribute());
		}
	}
}