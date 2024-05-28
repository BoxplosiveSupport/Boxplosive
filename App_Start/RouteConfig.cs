using System.Web.Mvc;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "ModifyLoyalty",
                url: "ModifyLoyalty/{id}",
                defaults: new { controller = "ModifyLoyalty", action = "Index" },
                constraints: new { id = @"\d+" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
