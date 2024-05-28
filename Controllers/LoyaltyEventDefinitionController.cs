using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.LoyaltyEventDefinition;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Sdk.Enums;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Feature("LoyaltyEvent", Order = 0)]
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LoyaltyEventDefinitionController : ControllerBase
    {
        private const string _LoyaltyEvent = "loyalty event";

        private readonly LoyaltyEventDefinitionFactory _Factory = new LoyaltyEventDefinitionFactory();

        public virtual ActionResult Index(int? page)
        {
            var loyaltyEventDefinitions =
                LoyaltyEventDefinitionApi.GetAll(page ?? PaginationModel.DefaultPageNumber, PaginationModel.DefaultPageSize);

            var model = _Factory.CreateLoyaltyEventDefinitionsModel(loyaltyEventDefinitions, Url);

            return View(model);
        }

        public virtual ActionResult Create()
        {
            var model = _Factory.CreateLoyaltyEventDefinitionModel(loyaltyEventDefinition: null, urlHelper: Url);
            return View(MVC.LoyaltyEventDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(LoyaltyEventDefinitionModel model)
        {
            return Save(model);
        }

        public virtual ActionResult Edit(int id, int? page)
        {
            var loyaltyEventDefinition = LoyaltyEventDefinitionApi.Get(id);
            var model = _Factory.CreateLoyaltyEventDefinitionModel(loyaltyEventDefinition, Url, page);

            return View(MVC.LoyaltyEventDefinition.Views.CreateOrEdit, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LoyaltyEventDefinitionModel model)
        {
            return Save(model);
        }

        public virtual ActionResult Delete(LoyaltyEventDefinitionModel model)
        {
            try
            {
                LoyaltyEventDefinitionApi.Delete(model.Id, model.Version);
            }
            catch (SqlOptimisticConcurrencyException)
            {
                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, _LoyaltyEvent);
            }

            return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
        }

        public virtual ActionResult GetTypePartialView(LoyaltyEventType? selected)
        {
            return PartialView(LoyaltyEventDefinitionModel.GetTypePartialViewName(selected), new LoyaltyEventDefinitionModel());
        }

        public virtual ActionResult GetActionPartialView(LoyaltyEventAction? selected)
        {
            return PartialView(LoyaltyEventDefinitionModel.GetActionPartialViewName(selected), new LoyaltyEventDefinitionModel());
        }

        private ActionResult Save(LoyaltyEventDefinitionModel model)
        {
            if (model.IsValid(ModelState))
            {
                try
                {
                    LoyaltyEventDefinition loyaltyEventDefinition = _Factory.ModelToLoyaltyEventDefinition(model);
                    LoyaltyEventDefinitionApi.InsertOrUpdate(loyaltyEventDefinition);

                    return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
                }
                catch (SqlOptimisticConcurrencyException)
                {
                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, _LoyaltyEvent);
                }
            }

            _Factory.SetPageNavigationValuesForDetailPage(model, Url);
            return View(MVC.LoyaltyEventDefinition.Views.CreateOrEdit, model);
        }
    }
}