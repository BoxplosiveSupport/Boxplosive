using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.ManageNewsArticle;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.BackOffice.Business.Data;
using nl.boxplosive.BackOffice.Business.Data.Entities;
using nl.boxplosive.Service.ServiceContract.FileService.DataContracts;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [boxplosive.BackOffice.Mvc.Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class ManageNewsArticleController : PlatformController
    {
        // GET: ManageNewsArticle
        public virtual ActionResult Index()
        {
            var model = NewsArticleFactory.CreateManageNewsArticleModel("Manage news articles", PageConst.Title_NavigateToHomePage, Url.Action(MVC.Home.ActionNames.Index, MVC.Home.Name));

            return View(model);
        }

        public virtual ActionResult Edit(int id = 0)
        {
            NewsArticleModel model = NewsArticleFactory.CreateNewsArticleModel(id, "Manage news article", PageConst.Title_NavigateToPreviousPage, Url.Action("Index"));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public virtual ActionResult Edit(NewsArticleModel model)
        {
            if (ModelState.IsValid)
            {
                // Insert or update news article
                DtoNewsArticle dtoNewsArticle = model.CopyValuesToDataModel();
                dtoNewsArticle = NewsArticleRepository.InsertOrUpdateNewsArticle(dtoNewsArticle);
                model.MapId(dtoNewsArticle);

                // Set news article placeholders values (for use in template)
                IList<DtoPlaceholderValue> dataPlaceholders = model.ToDataObject_PlaceholderValues();
                foreach (DtoPlaceholderValue dataPlaceholder in dataPlaceholders)
                {
                    PlaceholderRepository.SetValue(dataPlaceholder);
                }

                return RedirectToAction(ActionNames.Index);
            }

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Delete(int id)
        {
            NewsArticleModel model = NewsArticleFactory.CreateNewsArticleModel(id, "Manage news article", PageConst.Title_NavigateToPreviousPage, Url.Action("Index"));

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(NewsArticleModel newsArticle)
        {
            int id = newsArticle.Id;
            NewsArticleModel model = NewsArticleFactory.CreateNewsArticleModel(id);

            // Delete news article dependencies
            PlaceholderRepository.DeleteValuesByNewsArticle(id);

            // Delete news article
            NewsArticleRepository.DeleteNewsArticle(id);

            return RedirectToAction(ActionNames.Index);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UploadImage1()
        {
            var model = new NewsArticleModel();
            if (ImageHelper.ValidateAndUpload(out SaveFileResponse uploadResult, Request.Files["Image1Upload"], ModelState))
                model.ImageUrl = uploadResult.Url;
            else
                model.ImageUrl = string.Empty;

            return Json(model);
        }

        #region Placeholder

        public virtual PartialViewResult GetPlaceholders(int newsArticleId, int? templateId)
        {
            var model = NewsArticleFactory.GetPlaceholderModels(newsArticleId, templateId);

            return PartialView("Placeholders", model);
        }

        #endregion Placeholder

        #region Repositories

        protected static INewsArticleRepository NewsArticleRepository => DataRepositoryFactory.GetInstance().DataRepository<INewsArticleRepository>();

        #endregion Repositories
    }
}