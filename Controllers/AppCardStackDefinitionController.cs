using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.AppCardStackDefinition;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, IssuerAdmin")]
    public partial class AppCardStackDefinitionController : PlatformController
    {
        private readonly AppCardStackDefinitionFactory _Factory = new AppCardStackDefinitionFactory();

        public virtual ActionResult Index(int? page)
        {
            var definitions = AppCardStackDefinitionRepository.GetPage(page ?? PaginationModel.DefaultPageNumber, PaginationModel.DefaultPageSize);
            var model = _Factory.CreateListPageModel(definitions, Url);

            return View(model);
        }

        public virtual ActionResult Create()
        {
            var model = _Factory.CreateDetailPageModel(item: null, urlHelper: Url);
            return View(MVC.AppCardStackDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(AppCardStackDefinitionModel model)
        {
            return Save(model);
        }

        public virtual ActionResult Edit(int id, int? page)
        {
            var definition = AppCardStackDefinitionRepository.Get(id);
            var model = _Factory.CreateDetailPageModel(definition, Url, page);

            return View(MVC.AppCardStackDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(AppCardStackDefinitionModel model)
        {
            return Save(model);
        }

        private ActionResult Save(AppCardStackDefinitionModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var definition = _Factory.ModelToBusiness(model);
                    AppCardStackDefinitionRepository.InsertOrUpdate(definition);

                    return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
                }
                catch (SqlOptimisticConcurrencyException)
                {
                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, AppCardStackDefinitionFactory.ItemPageTitle.ToLower());
                }
            }

            _Factory.SetPageNavigationValuesForDetailPage(model, Url);
            return View(MVC.AppCardStackDefinition.Views.CreateOrEdit, model);
        }
    }
}