using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageGroup;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using NLog;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LanguageMessageGroupController : ControllerBase
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        public static readonly string PageTitle_Index = "Language message groups";
        public static readonly string PageTitle_Create = $"Create {PageTitle_Index}";
        public static readonly string PageTitle_Edit = $"Edit {PageTitle_Index}";
        public static readonly string PageTitle_Delete = $"Delete {PageTitle_Index}";

        private static readonly string _TenantId;

        static LanguageMessageGroupController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(string sortColumn, string sortOrder)
        {
            var errorMessages = new List<string>();
            var model = new LanguageMessageGroupIndexModel(_TenantId, sortColumn, sortOrder);
            if (!model.LanguageMessageGroupsGetResult_IsSuccess)
                errorMessages.Add("Failed to retrieve language message groups, try again later.");

            SetIndexModelPageFields(model);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new LanguageMessageGroupCreateModel();
            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(LanguageMessageGroupCreateModel model)
        {
            LanguageMessageGroupPutResult languageMessageGroupPutResult = null;
            if (ModelState.IsValid)
            {
                try
                {
                    languageMessageGroupPutResult = model.SubmitCreate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to create language message group: id={model?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageMessageGroupPutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language message group created: id={languageMessageGroupPutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language '{model?.Name}' created.";

                    return RedirectToAction(MVC.LanguageMessageGroup.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to create language message group: responseStatusCode={languageMessageGroupPutResult?.StatusCode} responseId={languageMessageGroupPutResult?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to create '{model?.Name}' language message group.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new LanguageMessageGroupEditModel(_TenantId, id);
            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LanguageMessageGroupEditModel model)
        {
            LanguageMessageGroupPutResult languageMessageGroupPutResult = null;
            if (ModelState.IsValid)
            {
                try
                {
                    languageMessageGroupPutResult = model.SubmitUpdate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to update language message group: id={languageMessageGroupPutResult?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageMessageGroupPutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language message group updated: id={languageMessageGroupPutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language message group '{model?.Name}' updated.";

                    return RedirectToAction(MVC.LanguageMessageGroup.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to update language message group: responseStatusCode={languageMessageGroupPutResult?.StatusCode} responseId={languageMessageGroupPutResult?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to update '{model?.Name}' language message group.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        public virtual ActionResult Delete(LanguageMessageGroupModel model)
        {
            LanguageMessageGroupDeleteResult languageMessageGroupDeleteResult = null;
            try
            {
                languageMessageGroupDeleteResult = model.SubmitDelete(_TenantId);
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.ErrorException($"Failed to delete language message group: id={model?.Id} name={model?.Name}", ex);

                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NotificationDefinitionFactory.ItemPageTitle.ToLower());
            }

            if (languageMessageGroupDeleteResult?.IsSuccessStatusCode == true)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info($"Language message group deleted: id={model?.Id} name={model?.Name}");

                TempData["Success"] = $"Language message group '{model?.Name}' deleted.";
            }
            else if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to delete language message group: responseStatusCode={languageMessageGroupDeleteResult?.StatusCode} id={model?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to delete '{model?.Name}' language message group.";
            }

            return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
        }

        private void SetIndexModelPageFields(LanguageMessageGroupIndexModel model)
        {
            model.PageTitle = $"{LanguageNavController.PageTitle_Index} - {PageTitle_Index}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageNav.ActionNames.Index, MVC.LanguageNav.Name);
            model.PageNavigationUrlText = $"Back to {LanguageNavController.PageTitle_Index.ToLower()}";
        }

        private void SetCreateEditDeleteModelPageFields(LanguageMessageGroupModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{MVC.Management.Name} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageMessageGroup.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}
