using System;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Business.Sdk.Exceptions;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Controllers
{
    [Feature("WebSealEnabled", false, Order = 0)]
    [Attributes.Authorize(Roles = "ApplicationManager, ViewAdminSection", Order = 1)]
    public partial class IssuerController : Mvc.Controllers.ControllerBase
    {
        private static readonly string _Entity_Name = "Account";
        public static readonly string PageTitle_Index = $"{_Entity_Name}s";
        public static readonly string PageTitle_Create = $"Create {_Entity_Name}";
        public static readonly string PageTitle_Edit = $"Edit {_Entity_Name}";
        public static readonly string PageTitle_Delete = $"Delete {_Entity_Name}";

        [HttpGet]
        public virtual ActionResult Index(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            // ELP-8052: Only admin issuers (PermissionGroup=AdminIssuer) are allowed to see admin accounts
            //   Note that the 'ViewAdminSection' permission is part of the 'AdminIssuer' group
            bool excludeAdminAccounts = !User.IsInRole("ViewAdminSection");
            int? pageSize = null;
            var model = new IssuerIndexModel(excludeAdminAccounts, page, pageSize, searchColumn, searchValue, sortColumn, sortOrder);
            SetIndexModelPageFields(model);

            return View(model);
        }

        [ActionName("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexPost(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            return RedirectToAction(MVC.AdminArea.Issuer.Index(page, searchColumn, searchValue, sortColumn, sortOrder));
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new IssuerCreateModel();
            SetCreateOrEditModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(IssuerCreateModel model)
        {
            return CreateOrEditPost(model, PageTitle_Create);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new IssuerEditModel(id);

            // ELP-8052: Not allowed to edit/delete admin issuers (PermissionGroup=AdminIssuer)
            //   This kind of issuers are not managed from within the backoffice
            if (model.IsAdmin)
            {
                TempData["Info"] = "Admin accounts cannot be edited.";

                return RedirectToAction(MVC.AdminArea.Issuer.Index());
            }

            SetCreateOrEditModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(IssuerEditModel model)
        {
            return CreateOrEditPost(model, PageTitle_Edit);
        }

        [HttpGet]
        public virtual ActionResult Delete(int id)
        {
            var model = new IssuerDeleteModel(id);

            // ELP-8052: Not allowed to edit/delete admin issuers (PermissionGroup=AdminIssuer)
            //   This kind of issuers are not managed from within the backoffice
            if (model.IsAdmin)
            {
                TempData["Info"] = "Admin accounts cannot be deleted.";

                return RedirectToAction(MVC.AdminArea.Issuer.Index());
            }

            SetDeleteModelPageFields(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(IssuerDeleteModel model)
        {
            return DeletePost(model);
        }

        private ActionResult CreateOrEditPost(IssuerModelBase model, string pageTitle)
        {
            bool isCreate = pageTitle.Equals(PageTitle_Create, StringComparison.OrdinalIgnoreCase);
            model.TrimFields(ModelState, isCreate);

            if (ModelState.IsValid)
            {
                bool modelStateIsValid = ModelState.IsValid;
                try
                {
                    model.CreateOrEdit(isCreate);
                }
                catch (IssuerAlreadyExistsException ex)
                {
                    modelStateIsValid = false;
                    model.SetIssuerAlreadyExistsModelError(ModelState);
                }
                catch (SqlOptimisticConcurrencyException)
                {
                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, _Entity_Name);
                }

                if (modelStateIsValid)
                    return RedirectToAction(MVC.AdminArea.Issuer.Index());
            }

            SetCreateOrEditModelPageFields(model, pageTitle);

            return View(model);
        }

        private ActionResult DeletePost(IssuerDeleteModel model)
        {
            try
            {
                model.Delete();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, _Entity_Name);
            }

            return RedirectToAction(MVC.AdminArea.Issuer.Index());
        }

        private void SetIndexModelPageFields(IssuerIndexModel model)
        {
            model.PageTitle = PageTitle_Index;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Admin.Index());
            model.PageNavigationUrlText = $"Back to {Area.ToLower()}";
        }

        private void SetCreateOrEditModelPageFields(IssuerModelBase model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.AdminArea.Issuer.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }

        private void SetDeleteModelPageFields(IssuerDeleteModel model)
        {
            model.PageTitle = PageTitle_Delete;
            model.PageNavigationTitle = $"{Area} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.AdminArea.Issuer.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}