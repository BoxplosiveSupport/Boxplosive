using System;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.LoyaltyPointsProgram;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using MessageType = nl.boxplosive.BackOffice.Mvc.Enums.MessageType;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class LoyaltyProgramController : ControllerBase
    {
        protected string ManagementPublicationsNavigationUrl => Url.Action(MVC.Management.ActionNames.Publications, MVC.Management.Name);

        private LoyaltyProgramModel InitializeModel(int programId, DraftableTab tab)
        {
            ModelState.Clear();

            return new LoyaltyProgramModel(programId, tab, ManagementPublicationsNavigationUrl);
        }

        private RedirectToRouteResult RedirectToLive(int loyaltyProgram, UiMessage message = null)
        {
            message?.SetMessageInTempData(TempData);

            return RedirectToAction(MVC.LoyaltyProgram.ActionNames.Live, new { loyaltyProgram });
        }

        private RedirectToRouteResult RedirectToStaging(int loyaltyProgram, UiMessage message = null)
        {
            message?.SetMessageInTempData(TempData);

            return RedirectToAction(MVC.LoyaltyProgram.ActionNames.Staging, new { loyaltyProgram });
        }

        public virtual ActionResult Index(int loyaltyProgram)
        {
            return RedirectToLive(loyaltyProgram);
        }

        public virtual ActionResult Live(int loyaltyProgram)
        {
            LoyaltyProgramModel model = InitializeModel(loyaltyProgram, DraftableTab.Live);

            if (!model.LiveTabAllowed())
                return Forbidden();

            model.Tab = DraftableTab.Live;

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Live(int loyaltyProgramId, string submitButton)
        {
            switch (submitButton)
            {
                case DraftActions.Edit:
                    return Edit(loyaltyProgramId);

                default:
                    throw new InvalidOperationException(submitButton);
            }
        }

        public virtual ActionResult Staging(int loyaltyProgram)
        {
            LoyaltyProgramModel model = InitializeModel(loyaltyProgram, DraftableTab.Staging);

            if (!model.StagingTabAllowed())
                return Forbidden();

            if (!model.SomeDraftRecordsPresent)
                return RedirectToLive(loyaltyProgram);

            model.Tab = DraftableTab.Staging;

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Staging(int loyaltyProgram, LoyaltyProgramModel model, string submitButton)
        {
            switch (submitButton)
            {
                case DraftActions.Ready:
                    return Ready(model);

                case DraftActions.SaveDraft:
                    return SaveDraft(model);

                case DraftActions.Approve:
                    return Approve(model);

                case DraftActions.Decline:
                    return Decline(model);

                case DraftActions.DeleteDraft:
                    return DeleteDraft(model);

                case DraftActions.Cancel:
                    return Cancel(model.LoyaltyProgramId);

                default:
                    throw new InvalidOperationException(submitButton);
            }
        }

        private ActionResult Edit(int loyaltyProgramId)
        {
            LoyaltyProgramModel model = InitializeModel(loyaltyProgramId, DraftableTab.Live);

            if (!model.UserHasEditPermission)
                return Forbidden();

            try
            {
                model.Edit();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, DraftMessages.Draft))
                );
            }

            return RedirectToStaging(loyaltyProgramId);
        }

        private ActionResult Ready(LoyaltyProgramModel model)
        {
            if (!model.UserHasEditPermission)
                return Forbidden();

            if (!ValidateModel(model))
            {
                return View(model);
            }

            try
            {
                model.Ready();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Readying, DraftMessages.Draft))
                );
            }

            model = InitializeModel(model.LoyaltyProgramId, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftReadyMessage));
        }

        private ActionResult SaveDraft(LoyaltyProgramModel model)
        {
            if (!model.UserHasEditPermission && !model.UserHasApprovePermission)
                return Forbidden();

            if (!ValidateModel(model))
            {
                return View(model);
            }

            try
            {
                model.Save();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, DraftMessages.Draft))
                );
            }

            model = InitializeModel(model.LoyaltyProgramId, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftSavedMessage));
        }

        private ActionResult Approve(LoyaltyProgramModel model)
        {
            if (!model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Approve();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Approving, DraftMessages.Draft))
                );
            }

            return RedirectToLive(model.LoyaltyProgramId, new UiMessage(MessageType.Success, DraftMessages.DraftApprovedMessage));
        }

        private ActionResult Decline(LoyaltyProgramModel model)
        {
            if (!model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Decline();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Declining, DraftMessages.Draft))
                );
            }

            model = InitializeModel(model.LoyaltyProgramId, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftDeclinedMessage));
        }

        private ActionResult DeleteDraft(LoyaltyProgramModel model)
        {
            if (!model.UserHasEditPermission && !model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Delete();
            }
            catch (SqlOptimisticConcurrencyException)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, DraftMessages.Draft))
                );
            }

            return RedirectToLive(model.LoyaltyProgramId, new UiMessage(MessageType.Success, DraftMessages.DraftDeletedMessage));
        }

        private ActionResult Cancel(int loyaltyProgramId)
        {
            return RedirectToLive(loyaltyProgramId);
        }

        private bool ValidateModel(LoyaltyProgramModel model)
        {
            if (!model.Draft.SharePointsEnabled)
            {
                ModelState.Clear();
            }
            else
            {
                if (!ModelState.IsValid)
                {
                    if (ModelState.TryGetValue("Draft.SharePointsValidityPeriodText", out ModelState sharePointsValidityPeriodTextValue) &&
                        sharePointsValidityPeriodTextValue.Errors.Any(e => e.Exception is FormatException))
                    {
                        ModelState.AddModelError("Draft.SharePointsValidityPeriodText", "The value must be in days or in a timespan format e.g. d.hh:mm.");
                    }
                }
                else if (model.Draft.SharePointsValidityPeriodText == null)
                {
                    ModelState.AddModelError("Draft.SharePointsValidityPeriodText", "The field is required.");
                }

                if (model.Draft.SharePointsMessageSubject == null)
                {
                    ModelState.AddModelError("Draft.SharePointsMessageSubject", "This field is required.");
                }

                if (model.Draft.SharePointsMessageBody == null)
                {
                    ModelState.AddModelError("Draft.SharePointsMessageBody", "This field is required.");
                }
            }

            return ModelState.IsValid;
        }
    }
}