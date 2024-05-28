using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Segmentation;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class SegmentationController : ControllerBase
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        public static readonly string PageTitle_Index = $"{NameConst}s";
        public static readonly string PageTitle_Create = $"Create {NameConst}";
        public static readonly string PageTitle_Edit = $"Edit {NameConst}";
        public static readonly string PageTitle_Delete = $"Delete {NameConst}";

        private static readonly string _TenantId;

        static SegmentationController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            int? pageSize = null;
            var model = new SegmentationIndexModel(page, pageSize, searchColumn, searchValue, sortColumn, sortOrder);
            SetIndexModelPageFields(model);

            return View(model);
        }

        [ActionName("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexPost(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder)
        {
            return RedirectToAction(MVC.Segmentation.Index(page, searchColumn, searchValue, sortColumn, sortOrder));
        }

        [HttpGet]
        public virtual ActionResult Create()
        {
            var model = new SegmentationCreateModel();
            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Create(SegmentationCreateModel model)
        {
            if (ModelState.IsValid)
            {
                DtoSegment segmentDto = null;
                try
                {
                    segmentDto = model.SubmitCreate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Logger.IsErrorEnabled)
                        _Logger.Error(ex, $"Failed to create segment: id={model?.Id} culture={model?.Name}");

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (segmentDto != null)
                {
                    if (_Logger.IsInfoEnabled)
                        _Logger.Info($"Segment created: id={segmentDto?.Id} name={segmentDto?.Name}");

                    TempData["Success"] = $"Segment '{segmentDto?.Name}' created.";

                    return RedirectToAction(MVC.Segmentation.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Logger.IsErrorEnabled)
                    _Logger.Error($"Failed to create segment: id={model?.Id} culture={model?.Name}");


                TempData["Danger"] = $"Failed to create '{model?.Name}' segment.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Create);

            return View(model);
        }

        [HttpGet]
        public virtual ActionResult Edit(int id)
        {
            var model = new SegmentationEditModel(_TenantId, id);
            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(SegmentationEditModel model)
        {
            if (ModelState.IsValid)
            {
                DtoSegment segmentDto = null;
                try
                {
                    segmentDto = model.SubmitUpdate(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Logger.IsErrorEnabled)
                        _Logger.Error(ex, $"Failed to update segment: id={model?.Id} culture={model?.Name}");

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, NameConst);
                }

                if (segmentDto != null)
                {
                    if (_Logger.IsInfoEnabled)
                        _Logger.Info($"Segment updated: id={segmentDto?.Id} culture={segmentDto?.Name}");

                    TempData["Success"] = $"Segment '{model?.Name}' updated.";

                    return RedirectToAction(MVC.Segmentation.Index());
                }
            }

            if (!TempData.ContainsKey("Danger"))
            {
                if (_Logger.IsErrorEnabled)
                    _Logger.Error($"Failed to update segment: id={model?.Id} name={model?.Name}");


                TempData["Danger"] = $"Failed to update '{model?.Name}' segment.";
            }

            SetCreateEditDeleteModelPageFields(model, PageTitle_Edit);

            return View(model);
        }

        public virtual ActionResult Delete(SegmentationModel model)
        {
            // Only allow delete when segment is not used
            bool modelIsValid = true;
            (bool yes, IList<DtoDistribution> distributions) canDeleteResult = model.CanDelete(_TenantId);
            if (!canDeleteResult.yes)
            {
                if (_Logger.IsErrorEnabled)
                    _Logger.Error($"Failed to delete segment: reason=SegmentInUse distributionIds=[{string.Join(", ", canDeleteResult.distributions.Select(item => item.Id).ToList())}] id={model?.Id} name={model?.Name}");

                TempData["Danger"] = $"Failed to delete '{model?.Name}' segment. Segment is in use by these promotions: {string.Join(", ", canDeleteResult.distributions.Select(item => $"'{item.PromotionId}'").ToList())}.";

                modelIsValid = false;
            }

            DtoSegment segmentDto = null;
            if (modelIsValid)
            {
                try
                {
                    segmentDto = model.SubmitDelete(_TenantId);
                }
                catch (SqlOptimisticConcurrencyException ex)
                {
                    if (_Logger.IsErrorEnabled)
                        _Logger.Error(ex, $"Failed to delete segment: id={model?.Id} culture={model?.Name}");

                    TempData["Danger"] = ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, NotificationDefinitionFactory.ItemPageTitle.ToLower());
                }
            }

            if (segmentDto != null)
            {
                if (_Logger.IsInfoEnabled)
                    _Logger.Info($"Segment deleted: id={segmentDto?.Id} culture={segmentDto?.Name}");

                TempData["Success"] = $"Segment '{segmentDto?.Name}' deleted.";
            }
            else if (!TempData.ContainsKey("Danger"))
            {
                if (_Logger.IsErrorEnabled)
                    _Logger.Error($"Failed to delete segment: id={model?.Id} name={model?.Name}");

                TempData["Danger"] = $"Failed to delete '{model?.Name}' segment.";
            }

            return RedirectToAction(MVC.Segmentation.Index());
        }

        private void SetIndexModelPageFields(SegmentationIndexModel model)
        {
            model.PageTitle = $"{MVC.Management.Name} - {PageTitle_Index}";
            model.PageNavigationUrl = Url.Action(MVC.Management.ActionNames.Index, MVC.Management.Name);
            model.PageNavigationUrlText = PageConst.Title_NavigateToManagementPage;
        }

        private void SetCreateEditDeleteModelPageFields(SegmentationModel model, string pageTitle)
        {
            model.PageTitle = pageTitle;
            model.PageNavigationTitle = $"{MVC.Management.Name} - {model.PageTitle}";
            model.PageNavigationUrl = Url.Action(MVC.Segmentation.Index());
            model.PageNavigationUrlText = $"Back to {PageTitle_Index.ToLower()}";
        }
    }
}
