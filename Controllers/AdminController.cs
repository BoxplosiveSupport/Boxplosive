using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Admin;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    /// <summary>
    /// Admin section which is only accessible to Boxplosive employees
    /// </summary>
    [Attributes.Authorize(Roles = "ApplicationManager, ViewAdminSection")]
    public partial class AdminController : ControllerBase
    {
        public virtual ActionResult Index()
        {
            var model = new AdminModel();
            model.PageTitle = MVC.Admin.Name;
            model.PageNavigationUrl = Url.Action(MVC.Home.Index());
            model.PageNavigationUrlText = PageConst.Title_NavigateToHomePage;

            return View(model);
        }
    }
}