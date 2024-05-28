using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.LanguageDeliveryProvider;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using NLog;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class LanguageDeliveryProviderController : ControllerBase
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        public static readonly string PageTitle_Index = "Language delivery providers";
        public static readonly string PageTitle_Create = $"Create {PageTitle_Index}";
        public static readonly string PageTitle_Edit = $"Edit {PageTitle_Index}";
        public static readonly string PageTitle_Delete = $"Delete {PageTitle_Index}";

        private static readonly string _TenantId;

        static LanguageDeliveryProviderController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(string sortColumn, string sortOrder)
        {
            var errorMessages = new List<string>();
            var model = new LanguageDeliveryProviderIndexModel(_TenantId, sortColumn, sortOrder);
            if (!model.LanguageDeliveryProvidersGetResult_IsSuccess)
                errorMessages.Add("Failed to retrieve language delivery providers, try again later.");

            SetIndexModelPageFields(model);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new LanguageDeliveryProviderCreateModel();
            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(LanguageDeliveryProviderCreateModel model)
        {
            model.ValidatePropertiesByDeliveryProviderName(out bool modelPropertiesIsValid);
            bool modelIsValid = ModelState.IsValid && modelPropertiesIsValid;

            LanguageDeliveryProviderPutResult languageDeliveryProviderPutResult = null;
            if (modelIsValid)
            {
                try
                {
                    languageDeliveryProviderPutResult = model.SubmitCreate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to create language delivery provider: id={model?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageDeliveryProviderPutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language delivery provider created: id={languageDeliveryProviderPutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language '{model?.Name}' created.";

                    return RedirectToAction(MVC.LanguageDeliveryProvider.Index());
                }
            }

            if (modelIsValid && !TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to create language delivery provider: responseStatusCode={languageDeliveryProviderPutResult?.StatusCode} responseId={languageDeliveryProviderPutResult?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to create '{model?.Name}' language delivery provider.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new LanguageDeliveryProviderEditModel(_TenantId, id);
            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(LanguageDeliveryProviderEditModel model)
        {
            model.ValidatePropertiesByDeliveryProviderName(out bool modelPropertiesIsValid);
            bool modelIsValid = ModelState.IsValid && modelPropertiesIsValid;

            LanguageDeliveryProviderPutResult languageDeliveryProviderPutResult = null;
            if (modelIsValid)
            {
                try
                {
                    languageDeliveryProviderPutResult = model.SubmitUpdate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Log.IsErrorEnabled)
                        _Log.ErrorException($"Failed to update language delivery provider: id={languageDeliveryProviderPutResult?.Id} name={model?.Name}", ex);

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (languageDeliveryProviderPutResult?.IsSuccess == true)
                {
                    if (_Log.IsInfoEnabled)
                        _Log.Info($"Language delivery provider updated: id={languageDeliveryProviderPutResult?.Id} name={model?.Name}");

                    TempData["Success"] = $"Language delivery provider '{model?.Name}' updated.";

                    return RedirectToAction(MVC.LanguageDeliveryProvider.Index());
                }
            }

            if (modelIsValid && !TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to update language delivery provider: responseStatusCode={languageDeliveryProviderPutResult?.StatusCode} responseId={languageDeliveryProviderPutResult?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to update '{model?.Name}' language delivery provider.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        public virtual ActionResult Delete(LanguageDeliveryProviderModel model)
        {
            LanguageDeliveryProviderDeleteResult languageDeliveryProviderDeleteResult = null;
            try
            {
                languageDeliveryProviderDeleteResult = model.SubmitDelete(_TenantId);
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                if (_Log.IsErrorEnabled)
                    _Log.ErrorException($"Failed to delete language delivery provider: id={model?.Id} name={model?.Name}", ex);

                TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NotificationDefinitionFactory.ItemPageTitle.ToLower());
            }

            if (languageDeliveryProviderDeleteResult?.IsSuccessStatusCode == true)
            {
                if (_Log.IsInfoEnabled)
                    _Log.Info($"Language delivery provider deleted: id={model?.Id} culture={model?.Name}");

                TempData["Success"] = $"Language delivery provider '{model?.Name}' deleted.";
            }
            else if (!TempData.ContainsKey("Danger"))
            {
                if (_Log.IsErrorEnabled)
                    _Log.Error($"Failed to delete language delivery provider: responseStatusCode={languageDeliveryProviderDeleteResult?.StatusCode} id={model?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to delete '{model?.Name}' language delivery provider.";
            }

            return RedirectToAction(MVC.LoyaltyEventDefinition.ActionNames.Index, new { model.Page });
        }

        private void SetIndexModelPageFields(LanguageDeliveryProviderIndexModel model)
        {
            model.PageTitle = $"{LanguageNavController.PageTitle_Index} - {PageTitle_Index}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageNav.ActionNames.Index, MVC.LanguageNav.Name);
            model.PageNavigationUrlText = $"Back to {LanguageNavController.PageTitle_Index.ToLower()}";
        }

        private void SetCreateEditDeleteModelPageFields(LanguageDeliveryProviderModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{MVC.Management.Name} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.LanguageDeliveryProvider.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }

        public virtual ActionResult GetNamePartialView(string selected)
        {
            return PartialView(LanguageDeliveryProviderModel.GetNamePartialViewName(selected), new LanguageDeliveryProviderCreateModel()
            {
                Name = selected,
                Properties = LanguageDeliveryProviderModel.GetPropertiesByDeliveryProviderName(selected),
            });
        }
    }
}
