using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Language;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Data.Web.Language.Models.Language;
using NLog;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LanguageController : ControllerBase
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        public static readonly string PageTitle_Index = $"{NameConst}s";
        public static readonly string PageTitle_Create = $"Create {NameConst}";
        public static readonly string PageTitle_Edit = $"Edit {NameConst}";
        public static readonly string PageTitle_Delete = $"Delete {NameConst}";

        private static readonly string _TenantId;

        static LanguageController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(string sortColumn, string sortOrder)
        {
            var errorMessages = new List<string>();
            var model = new LanguageIndexModel(_TenantId, sortColumn, sortOrder);
            if (!model.LanguagesGetResult_IsSuccess)
                errorMessages.Add("Failed to retrieve languages, try again later.");

            SetIndexModelPageFields(model);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new LanguageCreateModel();
            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(LanguageCreateModel model)
        {
            LanguagePutResult languagePutResult = null;
            if (ModelState.IsValid)
            {
                try
                {
                    languagePutResult = model.SubmitCreate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to create language: id={model?.Id} culture={model?.Culture}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languagePutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language created: id={languagePutResult?.Id} culture={model?.Culture}");

                    TempData["Success"] = $"Language '{model?.Culture}' created.";

                    return RedirectToAction(MVC.Language.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to create language: responseStatusCode={languagePutResult?.StatusCode} responseId={languagePutResult?.Id} culture={model?.Culture}");


                TempData["Danger"] = $"Failed to create '{model?.Culture}' language.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new LanguageEditModel(_TenantId, id);
            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LanguageEditModel model)
        {
            LanguagePutResult languagePutResult = null;
            if (ModelState.IsValid)
            {
                try
                {
                    languagePutResult = model.SubmitUpdate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to update language: id={languagePutResult?.Id} culture={model?.Culture}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languagePutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language updated: id={languagePutResult?.Id} culture={model?.Culture}");

                    TempData["Success"] = $"Language '{model?.Culture}' updated.";

                    return RedirectToAction(MVC.Language.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to update language: responseStatusCode={languagePutResult?.StatusCode} responseId={languagePutResult?.Id} culture={model?.Culture}");


                TempData["Danger"] = $"Failed to update '{model?.Culture}' language.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        public virtual ActionResult Delete(LanguageModel model)
        {
            LanguageDeleteResult languageDeleteResult = null;
            try
            {
                languageDeleteResult = model.SubmitDelete(_TenantId);
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.ErrorException($"Failed to delete language: id={model?.Id} culture={model?.Culture}", ex);

                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NotificationDefinitionFactory.ItemPageTitle.ToLower());
            }

            if (languageDeleteResult?.IsSuccessStatusCode == true)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info($"Language deleted: id={model?.Id} culture={model?.Culture}");

                TempData["Success"] = $"Language '{model?.Culture}' deleted.";
            }
            else if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to delete language: responseStatusCode={languageDeleteResult?.StatusCode} id={model?.Id} culture={model?.Culture}");


                TempData["Danger"] = $"Failed to delete '{model?.Culture}' language.";
            }

            return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
        }

        private void SetIndexModelPageFields(LanguageIndexModel model)
        {
            model.PageTitle = $"{LanguageNavController.PageTitle_Index} - {PageTitle_Index}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageNav.ActionNames.Index, MVC.LanguageNav.Name);
            model.PageNavigationUrlText = $"Back to {LanguageNavController.PageTitle_Index.ToLower()}";
        }

        private void SetCreateEditDeleteModelPageFields(LanguageModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{MVC.Management.Name} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Language.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}
