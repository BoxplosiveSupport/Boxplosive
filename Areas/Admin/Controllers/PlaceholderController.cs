using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Controllers
{
    /// <summary>
    /// Controller for the placeholder management page
    /// </summary>
    [Attributes.Authorize(Roles = "ViewAdminSection")]
    public partial class PlaceholderController : Mvc.Controllers.ControllerBase
    {
        public static readonly string PageTitle_Index = $"{NameConst}s";
        public static readonly string PageTitle_Create = $"Create {NameConst}";

        [HttpGet]
        public virtual ActionResult Index()
        {
            var model = new PlaceholderIndexModel();
            SetIndexModelPageFields(model);
            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new PlaceholderCreateModel();
            SetCreateModelPageFields(model, PageTitle_Create);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(PlaceholderCreateModel model)
        {
            return CreatePost(model, PageTitle_Create);
        }

        private ActionResult CreatePost(PlaceholderCreateModel model, string pageTitle)
        {
            if (model.FieldNameAlreadyExists())
            {
                ModelState.AddModelError(nameof(model.FieldName), $"FieldName already exists, please choose a different FieldName");
            }

            if (model.FieldNameUsesMappedPlaceholder())
            {
                ModelState.AddModelError(nameof(model.FieldName), $"FieldName cannot be equal to a mapped placeholder");
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

                return RedirectToAction(MVC.AdminArea.Placeholder.Index());
            }

            SetCreateModelPageFields(model, pageTitle);

            return View(model);
        }

        private void SetIndexModelPageFields(PlaceholderIndexModel model)
        {
            model.PageTitle = PageTitle_Index;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Admin.Index());
            model.PageNavigationUrlText = $"Back to {Area.ToLower()}";
        }

        private void SetCreateModelPageFields(PlaceholderCreateModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.AdminArea.Placeholder.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}