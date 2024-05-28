using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using System;
using System.IO;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class HomeController : ControllerBase
    {
        private static int PageSize = 10;

        public virtual ActionResult Index()
        {
            var model = new HomeModel
            {
                PageTitle = "Home",
                Create = HomeFactory.CreateCreateModule(Url),
                Publish = PublishFactory.CreatePublishModel(CampaignType.All, Url),
                Analyse = AnalyseFactory.CreateAnalyseModel(Url),
                ClientModules = HomeFactory.CreateHomePageModules(Url)
            };

            return View(model);
        }

        public virtual ActionResult ShowAllLiveCampaigns(int? page)
        {
            var pageNumber = 1;
            if (page.HasValue && page.Value > 1)
                pageNumber = page.Value;

            var model = AnalyseFactory.CreateCampaignsModel(Url, pageNumber, PageSize, false, DateTime.UtcNow, DateTime.UtcNow);

            model.PageTitle = "Live campaigns";
            model.PageNavigationUrl = Url.Action(ActionNameConstants.Index, NameConst);
            model.PageNavigationUrlText = PageConst.Title_NavigateToHomePage;

            return View("ShowAllCampaigns", model);
        }

        public virtual ActionResult ShowAllFinishedCampaigns(int? page)
        {
            var pageNumber = 1;
            if (page.HasValue && page.Value > 1)
                pageNumber = page.Value;

            var model = AnalyseFactory.CreateCampaignsModel(Url, pageNumber, PageSize, true, DateTime.UtcNow, DateTime.UtcNow.AddDays(-30));

            model.PageTitle = "Finished campaigns";
            model.PageNavigationUrl = Url.Action(ActionNameConstants.Index, NameConst);
            model.PageNavigationUrlText = PageConst.Title_NavigateToHomePage;

            return View("ShowAllCampaigns", model);
        }

        public virtual ActionResult GetCampaignDrafts(int campaignType)
        {
            var type =
                (CampaignType)Enum.Parse(typeof(CampaignType), campaignType.ToString());
            var viewResult = RenderRazorViewToString(MVC.Home.Views.PublishModel, PublishFactory.CreatePublishModel(type, Url));
            return Json(viewResult, JsonRequestBehavior.AllowGet);
        }

        private string RenderRazorViewToString(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }
    }
}