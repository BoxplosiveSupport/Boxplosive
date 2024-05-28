using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin section which is only accessible to Boxplosive employees
    /// </summary>
    [Attributes.Authorize(Roles = "ViewAdminSection")]
    public partial class AppConfigItemController : Mvc.Controllers.ControllerBase
    {
        public static readonly string PageTitle_Index = $"{NameConst}s";

        public virtual ActionResult Index()
        {
            var model = new AppConfigItemIndexModel();
            model.PageTitle = PageTitle_Index;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Admin.Index());
            model.PageNavigationUrlText = $"Back to {Area.ToLower()}";

            return View(model);
        }
    }
}