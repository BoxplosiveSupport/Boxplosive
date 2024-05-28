using nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Utilities.Json;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Controllers
{
    /// <summary>
    /// Admin section which is only accessible to Boxplosive employees
    /// </summary>
    [Attributes.Authorize(Roles = "ViewAdminSection")]
    public partial class TemplateController : Mvc.Controllers.ControllerBase
    {
        public static readonly string PageTitle_Index = $"{NameConst}s";
        public static readonly string PageTitle_Create = $"Create {NameConst}";
        public static readonly string PageTitle_Edit = $"Edit {NameConst}";

        [HttpGet]
        public virtual ActionResult Index(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            int? pageSize = null;
            var model = new TemplateIndexModel(page, pageSize, searchColumn, searchValue, sortColumn, sortOrder);
            SetIndexModelPageFields(model);

            return View(model);
        }

        [ActionName("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexPost(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            return RedirectToAction(MVC.AdminArea.Template.Index(page, searchColumn, searchValue, sortColumn, sortOrder));
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new TemplateCreateModel();
            SetCreateOrEditModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(TemplateCreateModel model)
        {
            return CreateOrEditPost(model, PageTitle_Create);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new TemplateEditModel(id);
            SetCreateOrEditModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(TemplateEditModel model)
        {
            return CreateOrEditPost(model, PageTitle_Edit);
        }

        private ActionResult CreateOrEditPost(TemplateModelBase model, string pageTitle)
        {
            if (!JsonWrapper.IsValidJObject(model.Json))
            {
                ModelState.AddModelError(nameof(model.Json), "Invalid JSON format");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    model.CreateOrEdit();
                }
                catch (SqlOptimisticConcurrencyException)
                {
                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                return RedirectToAction(MVC.AdminArea.Template.Index());
            }

            SetCreateOrEditModelPageFields(model, pageTitle);

            return View(model);
        }

        private void SetIndexModelPageFields(TemplateIndexModel model)
        {
            model.PageTitle = PageTitle_Index;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Admin.Index());
            model.PageNavigationUrlText = $"Back to {Area.ToLower()}";
        }

        private void SetCreateOrEditModelPageFields(TemplateModelBase model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.AdminArea.Template.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}