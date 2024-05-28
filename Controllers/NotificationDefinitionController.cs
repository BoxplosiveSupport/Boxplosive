using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.NotificationDefinition;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Utilities.Json;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Feature(NotificationDefinitionFactory.FeatureName, Order = 0)]
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class NotificationDefinitionController : PlatformController
    {
        private readonly NotificationDefinitionFactory _Factory = new NotificationDefinitionFactory();

        public virtual ActionResult Index(int? page)
        {
            var definitions = NotificationDefinitionRepository.GetPage(page ?? PaginationModel.DefaultPageNumber, PaginationModel.DefaultPageSize);
            var model = _Factory.CreateListPageModel(definitions, Url);

            return View(model);
        }

        public virtual ActionResult Create()
        {
            var model = _Factory.CreateDetailPageModel(notificationDefinition: null, urlHelper: Url);
            return View(MVC.NotificationDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(NotificationDefinitionModel model)
        {
            return Save(model);
        }

        public virtual ActionResult Edit(int id, int? page)
        {
            var definition = NotificationDefinitionRepository.Get(id);
            var model = _Factory.CreateDetailPageModel(definition, Url, page);

            return View(MVC.NotificationDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(NotificationDefinitionModel model)
        {
            return Save(model);
        }

        private ActionResult Save(NotificationDefinitionModel model)
        {
            if (!JsonWrapper.IsValidJObject(model.Arguments))
            {
                ModelState.AddModelError("Arguments", ErrorMessageInvalidJsonFormat);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var definition = _Factory.ModelToBusiness(model);
                    NotificationDefinitionRepository.InsertOrUpdate(definition);

                    return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NotificationDefinitionFactory.ItemPageTitle.ToLower());
                }
            }

            _Factory.SetPageNavigationValuesForDetailPage(model, Url);
            return View(MVC.NotificationDefinition.Views.CreateOrEdit, model);
        }

        public virtual ActionResult Delete(NotificationDefinitionModel model)
        {
            try
            {
                NotificationDefinitionRepository.Delete(model.Id, model.Version);
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NotificationDefinitionFactory.ItemPageTitle.ToLower());
            }

            return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
        }
    }
}