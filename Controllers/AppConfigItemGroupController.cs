using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.AppConfigItems;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.AppConfigItem;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk.Exceptions.Sql;
using System;
using System.Linq;
using System.Web.Mvc;
using MessageType = nl.boxplosive.BackOffice.Mvc.Enums.MessageType;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class AppConfigItemGroupController : ControllerBase
    {
        protected IAppConfigItemApi AppConfigItemApi => BusinessApiFactory.GetInstance().BusinessApi<IAppConfigItemApi>();

        public const string ForgotToBringLoyaltyCardGroup = "ForgotToBringLoyaltyCard";

        private const string _ManageApplicationSettingsTitle = "Manage application settings";
        private const string _ManageFeatureSettingsTitle = "Manage {0} settings";

        private AppConfigItemGroupModel InitializeModel(string group, DraftableTab tab, bool clearModelState = true)
        {
            if (clearModelState)
            {
                ModelState.Clear();
            }

            var model = new AppConfigItemGroupModel(group, tab);
            model.SetPageFields(0, string.Format(_ManageFeatureSettingsTitle, group),
                Url.Action(MVC.AppConfigItemGroup.ActionNames.Index, MVC.AppConfigItemGroup.Name), PageConst.Title_NavigateToPreviousPage);
            return model;
        }

        private RedirectToRouteResult RedirectToLive(string group, UiMessage message = null)
        {
            message?.SetMessageInTempData(TempData);

            return RedirectToAction(MVC.AppConfigItemGroup.ActionNames.Live, new { group });
        }

        private RedirectToRouteResult RedirectToStaging(string group, UiMessage message = null)
        {
            message?.SetMessageInTempData(TempData);

            return RedirectToAction(MVC.AppConfigItemGroup.ActionNames.Staging, new { group });
        }

        // @todo: In future the index page will list all the groups
        public virtual ActionResult Index(string group = null)
        {
            if (!string.IsNullOrEmpty(group))
                RedirectToLive(group);

            var groups = AppConfigItemApi.GetGroups();

            var model = CreateAppConfigItemGroupsModel(groups);
            return View(model);
        }

        private AppConfigItemGroupsModel CreateAppConfigItemGroupsModel(AppConfigItemGroups groups)
        {
            var model = new AppConfigItemGroupsModel();

            foreach (var group in groups)
            {
                // If there is a feature toggle, it must be enabled
                if (!AppConfig.Settings.FeatureExists(group.Group) || AppConfig.Settings.IsFeatureEnabled(group.Group))
                {
                    model.AppConfigItemGroups.Add(group.Group);
                }
            }

            model.SetPageFields(0, _ManageApplicationSettingsTitle, Url.Action(MVC.Home.ActionNames.Index, MVC.Home.Name), PageConst.Title_NavigateToHomePage);
            return model;
        }

        public virtual ActionResult Live(string group)
        {
            AppConfigItemGroupModel model = InitializeModel(group, DraftableTab.Live);

            if (!model.LiveTabAllowed())
                return Forbidden();

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Live(string group, string submitButton)
        {
            switch (submitButton)
            {
                case DraftActions.Edit:
                    return Edit(group);

                default:
                    throw new InvalidOperationException(submitButton);
            }
        }

        public virtual ActionResult Staging(string group)
        {
            AppConfigItemGroupModel model = InitializeModel(group, DraftableTab.Staging);

            if (!model.StagingTabAllowed())
                return Forbidden();

            // Due to concurrent editing and deleting draft records might have been deleted.
            if (!model.SomeDraftRecordsPresent)
            {
                return RedirectToLive(model.Group);
            }

            return View(model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public virtual ActionResult Staging(string group, AppConfigItemGroupModel model, string submitButton)
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
                    return Cancel(model.Group);

                default:
                    throw new InvalidOperationException(submitButton);
            }
        }

        private ActionResult Edit(string group)
        {
            AppConfigItemGroupModel model = InitializeModel(group, DraftableTab.Live);

            if (!model.UserHasEditPermission)
                return Forbidden();

            try
            {
                model.Edit();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, DraftMessages.Draft)
                ));
            }

            return RedirectToStaging(group);
        }

        private ActionResult Ready(AppConfigItemGroupModel model)
        {
            if (!model.UserHasEditPermission)
                return Forbidden();

            try
            {
                model.Ready();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Readying, DraftMessages.Draft)
                ));
            }

            model = InitializeModel(model.Group, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftReadyMessage));
        }

        private ActionResult SaveDraft(AppConfigItemGroupModel model)
        {
            if (!model.UserHasEditPermission && !model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Save();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Saving, DraftMessages.Draft)
                ));
            }

            model = InitializeModel(model.Group, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftSavedMessage));
        }

        private ActionResult Approve(AppConfigItemGroupModel model)
        {
            if (!model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                DraftVersionsOccCheck(model);

                model.Approve();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Approving, DraftMessages.Draft)
                ));
            }

            return RedirectToLive(model.Group, new UiMessage(MessageType.Success, DraftMessages.DraftApprovedMessage));
        }

        private ActionResult Decline(AppConfigItemGroupModel model)
        {
            if (!model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Decline();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Declining, DraftMessages.Draft)
                ));
            }

            model = InitializeModel(model.Group, DraftableTab.Staging);

            return ViewAndMessage(model, new UiMessage(MessageType.Success, DraftMessages.DraftDeclinedMessage));
        }

        private ActionResult DeleteDraft(AppConfigItemGroupModel model)
        {
            if (!model.UserHasEditPermission && !model.UserHasApprovePermission)
                return Forbidden();

            try
            {
                model.Delete();
            }
            catch (SqlOptimisticConcurrencyException ex)
            {
                return ViewAndMessage(model, new UiMessage(
                    MessageType.Danger,
                    ValidationHelper.GetOccRecordModifiedMessage(OccValidationAction.Deleting, DraftMessages.Draft)
                ));
            }

            return RedirectToLive(model.Group, new UiMessage(MessageType.Success, DraftMessages.DraftDeletedMessage));
        }

        private ActionResult Cancel(string group)
        {
            return RedirectToLive(group);
        }

        private void DraftVersionsOccCheck(AppConfigItemGroupModel model)
        {
            AppConfigItemGroupModel dbModel = InitializeModel(model.Group, DraftableTab.Staging, clearModelState: false);

            foreach (IAppConfigItemDraftBso draft in model.Drafts)
            {
                var dbDraftVersion = dbModel.Drafts.FirstOrDefault(m => m.DraftId == draft.DraftId)?.DraftVersion;

                if (dbDraftVersion == null || !dbDraftVersion.SequenceEqual(draft.DraftVersion))
                {
                    throw new SqlOptimisticConcurrencyException(null);
                }
            }
        }
    }
}