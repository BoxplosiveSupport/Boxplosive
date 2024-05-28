using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.LanguageNav;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LanguageNavController : ControllerBase
    {
        public const string PageTitle_Index = "Languages";

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index()
        {
            return View(new LanguageNavModel(
                "Languages",
                PageConst.Title_NavigateToManagementPage,
                Url.Action(ManagementController.ActionNameConstants.Index, ManagementController.NameConst)
            ));
        }
    }
}
