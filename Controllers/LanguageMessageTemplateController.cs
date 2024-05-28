using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Language;
using nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageTemplate;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LanguageMessageTemplateController : ControllerBase
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        public static readonly string PageTitle_Index = "Language message templates";
        public static readonly string PageTitle_Create = $"Create {PageTitle_Index}";
        public static readonly string PageTitle_Edit = $"Edit {PageTitle_Index}";
        public static readonly string PageTitle_Delete = $"Delete {PageTitle_Index}";

        private static readonly string _TenantId;

        static LanguageMessageTemplateController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(string sortColumn, string sortOrder)
        {
            var errorMessages = new List<string>();
            var model = new LanguageMessageTemplateIndexModel(sortColumn, sortOrder);
            if (!model.LanguageMessageTemplatesGetResult_IsSuccess)
                errorMessages.Add("Failed to retrieve language message templates, try again later.");

            SetIndexModelPageFields(model);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new LanguageMessageTemplateCreateModel();
            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(LanguageMessageTemplateCreateModel model)
        {
            model.ValidatePropertiesByMessageTemplateName(out bool modelPropertiesIsValid);
            bool modelIsValid = ModelState.IsValid && modelPropertiesIsValid;

            var x = ModelState.Values.SelectMany(item => item.Errors);

            LanguageMessageTemplatePutResult languageMessageTemplatePutResult = null;
            if (modelIsValid)
            {
                try
                {
                    languageMessageTemplatePutResult = model.SubmitCreate();
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to create language message template: id={model?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageMessageTemplatePutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language message template created: id={languageMessageTemplatePutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language '{model?.Name}' created.";

                    return RedirectToAction(MVC.LanguageMessageTemplate.Index());
                }
            }

            if (modelIsValid && !TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to create language message template: responseStatusCode={languageMessageTemplatePutResult?.StatusCode} responseId={languageMessageTemplatePutResult?.Id} name={model?.Name}");

                TempData["Danger"] = $"Failed to create '{model?.Name}' language message template.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new LanguageMessageTemplateEditModel(id);
            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LanguageMessageTemplateEditModel model)
        {
            model.ValidatePropertiesByMessageTemplateName(out bool modelPropertiesIsValid);
            bool modelIsValid = ModelState.IsValid && modelPropertiesIsValid;

            LanguageMessageTemplatePutResult languageMessageTemplatePutResult = null;
            if (modelIsValid)
            {
                try
                {
                    languageMessageTemplatePutResult = model.SubmitUpdate();
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to update language message template: id={model?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageMessageTemplatePutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language message template updated: id={languageMessageTemplatePutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language message template '{model?.Name}' updated.";

                    return RedirectToAction(MVC.LanguageMessageTemplate.Index());
                }
            }

            if (modelIsValid && !TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to update language message template: responseStatusCode={languageMessageTemplatePutResult?.StatusCode} responseId={languageMessageTemplatePutResult?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to update '{model?.Name}' language message template.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        public virtual ActionResult Delete(LanguageMessageTemplateModel model)
        {
            LanguageMessageTemplateDeleteResult languageMessageTemplateDeleteResult = null;
            try
            {
                languageMessageTemplateDeleteResult = model.SubmitDelete();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.ErrorException($"Failed to delete language message template: id={model?.Id} name={model?.Name}", ex);

                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NameConst);
            }

            if (languageMessageTemplateDeleteResult?.IsSuccessStatusCode == true)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info($"Language message template deleted: id={model?.Id} culture={model?.Name}");

                TempData["Success"] = $"Language message template '{model?.Name}' deleted.";
            }
            else if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to delete language message template: responseStatusCode={languageMessageTemplateDeleteResult?.StatusCode} id={model?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to delete '{model?.Name}' language message template.";
            }

            return RedirectToAction(MVC.LanguageMessageTemplate.ActionNames.Index, new { model.Page });
        }

        private void SetIndexModelPageFields(LanguageMessageTemplateIndexModel model)
        {
            model.PageTitle = $"{LanguageNavController.PageTitle_Index} - {PageTitle_Index}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageNav.ActionNames.Index, MVC.LanguageNav.Name);
            model.PageNavigationUrlText = $"Back to {LanguageNavController.PageTitle_Index.ToLower()}";
        }

        private void SetCreateEditDeleteModelPageFields(LanguageMessageTemplateModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{MVC.Management.Name} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageMessageTemplate.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }

        public virtual ActionResult GetMessageGroupPartialView(string selected)
        {
            IList<LanguageModel> languages = LanguageMessageTemplateModel.GetLanguages();

            return PartialView(LanguageMessageTemplateModel.GetMessageGroupPartialViewName(selected), new LanguageMessageTemplateModel()
            {
                MessageGroupName = selected,
                Properties = LanguageMessageTemplateModel.GetPropertiesByMessageTemplateGroup(selected, languages),
            });
        }
    }
}