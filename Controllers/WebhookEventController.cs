using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Webhook;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Webhook.Models;
using nl.boxplosive.Utilities.Extensions;
using nl.boxplosive.Utilities.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
    public partial class WebhookEventController : ControllerBase
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        private string _SessionKey_ErrorMessage => $"{nameof(WebhookEventController)}:{nameof(IndexPost)}:ErrorMessage";
        private string _SessionKey_SuccessMessage => $"{nameof(WebhookEventController)}:{nameof(IndexPost)}:SuccessMessage";

        private static readonly string _TenantId;

        static WebhookEventController()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public delegate ActionResult IndexDelegate();

        [HttpGet]
        public virtual ActionResult Index(string eventType, int? pageSize, int? pageNumber, string eventState, string from, string to)
        {
            DateTime? fromDt = DateTime.TryParseExact(from, WebhookEventIndexModel.DateFormat, null, DateTimeStyles.AssumeLocal, out DateTime fromDtResult)
                ? fromDtResult.ToLocalBeginOfDayInOperatingTimeZone()
                : (DateTime?)null;
            DateTime? toDt = DateTime.TryParseExact(to, WebhookEventIndexModel.DateFormat, null, DateTimeStyles.AssumeLocal, out DateTime toDtResult)
                ? toDtResult.ToLocalEndOfDayInOperatingTimeZone()
                : (DateTime?)null;

            ValidateInput(fromDt, toDt, out bool inputIsValid, out WebhookEventIndexModel inputModel);
            if (!inputIsValid && inputModel != null)
                return View(inputModel);

            var errorMessages = new List<string>();
            var model = new WebhookEventIndexModel(eventType, pageSize, pageNumber, eventState, fromDt, toDt, returnEmptyModel: !inputIsValid);
            if (!model.WebhookEventsGetResult_IsSuccess)
                errorMessages.Add("Failed to retrieve webhook events, try again later.");

            SetIndexModelPageFields(model);

            string successMessage = Session[_SessionKey_SuccessMessage] as string;
            Session.Remove(_SessionKey_SuccessMessage);
            if (!string.IsNullOrWhiteSpace(successMessage))
                TempData["Success"] = successMessage;

            string errorMessage = Session[_SessionKey_ErrorMessage] as string;
            Session.Remove(_SessionKey_ErrorMessage);
            if (!string.IsNullOrWhiteSpace(errorMessage))
                errorMessages.Add(errorMessage);
            if (errorMessages.Count > 0)
                TempData["Danger"] = string.Join(" ", errorMessages);

            TempData["WebhookEventModel"] = model;

            return View(model);
        }

        [ActionName("Index")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult IndexPost(string eventType, int? pageSize, int? pageNumber, string eventState, DateTime? from, DateTime? to)
        {
            if (from.HasValue)
                from = from.Value.ToLocalBeginOfDayInOperatingTimeZone();
            if (to.HasValue)
                to = to.Value.ToLocalEndOfDayInOperatingTimeZone();

            ValidateInput(from, to, out bool inputIsValid, out WebhookEventIndexModel inputModel);
            if (!inputIsValid)
                return View(inputModel);

            bool replayEvents = !string.IsNullOrWhiteSpace(Request.Form.Get("ReplayFilerType"));
            if (replayEvents)
            {
                List<string> selectedWebhookEventsKeys =
                    Request.Form.AllKeys.Where(key => key.StartsWith("WebhookEventSelected_", StringComparison.OrdinalIgnoreCase)).ToList();
                var selectedWebhookEventIds = new List<string>();
                foreach (string key in selectedWebhookEventsKeys)
                {
                    bool webhookEventIsSelected = Request.Form.Get(key).IndexOf("true", StringComparison.OrdinalIgnoreCase) >= 0;
                    if (webhookEventIsSelected)
                    {
                        string webhookEventId = key.Split(new char[] { '_' }, 2)[1];
                        selectedWebhookEventIds.Add(webhookEventId);
                    }
                }

                WebhookEventsReplayPostInput webhookEventsReplayPostInput = null;
                bool webhookEventsReplayIsSuccess = false;
                try
                {
                    // Replay events by selection
                    if (selectedWebhookEventIds.Any())
                    {
                        webhookEventsReplayPostInput = new WebhookEventsReplayPostInput(requestId: Guid.NewGuid().ToString(), tenantId: _TenantId,
                                selectedItems: selectedWebhookEventIds);
                        // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
                        var webhookWebClient = WebClientManager.WebhookWebClient;
                        WebhookEventsReplayPostResult webhookEventsReplayPostResult =
                                Task.Run(() => webhookWebClient.PostWebhookEventsReplayAsync(webhookEventsReplayPostInput)).GetAwaiter().GetResult();
                        //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
                        //{
                        //    return await balanceWebClient.GetBalanceAsync(webModel);
                        //}).Result;

                        if (webhookEventsReplayPostResult != null && (int)webhookEventsReplayPostResult.StatusCode >= 200
                                && (int)webhookEventsReplayPostResult.StatusCode <= 299)
                        {
                            webhookEventsReplayIsSuccess = true;
                            Session[_SessionKey_SuccessMessage] = $"Your request has been received, it can take a while before the '{selectedWebhookEventIds?.Count}' events in your selection have been processed.";
                        }
                    }
                    // replay events by filter
                    else
                    {
                        var model = new WebhookEventIndexModel(eventType, pageSize, pageNumber, eventState, from, to, returnEmptyModel: false);
                        if (model.WebhookEventsGetResult_IsSuccess)
                        {
                            webhookEventsReplayPostInput = new WebhookEventsReplayPostInput(requestId: Guid.NewGuid().ToString(), tenantId: _TenantId,
                                    from: model.From.ToUtcInOperatingTimeZone(), to: model.To.ToUtcInOperatingTimeZone(), eventType: model.EventType,
                                    state: model.EventState);
                            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
                            var webhookWebClient = WebClientManager.WebhookWebClient;
                            WebhookEventsReplayPostResult webhookEventsReplayPostResult =
                                    Task.Run(() => webhookWebClient.PostWebhookEventsReplayAsync(webhookEventsReplayPostInput)).GetAwaiter().GetResult();
                            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
                            //{
                            //    return await balanceWebClient.GetBalanceAsync(webModel);
                            //}).Result;

                            if (webhookEventsReplayPostResult != null && (int)webhookEventsReplayPostResult.StatusCode >= 200
                                && (int)webhookEventsReplayPostResult.StatusCode <= 299)
                            {
                                webhookEventsReplayIsSuccess = true;
                                Session[_SessionKey_SuccessMessage] = $"Your request has been received, it can take a while before the '{model?.PagedList?.TotalItemCount}' events in your filter have been processed.";
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    string requestInput = webhookEventsReplayPostInput != null ? JsonWrapper.SerializeObject(webhookEventsReplayPostInput) : null;
                    _Logger.Error(ex, $"Failed to replay webhook events: requestInput={requestInput}");
                }

                if (!webhookEventsReplayIsSuccess)
                    Session[_SessionKey_ErrorMessage] = "Failed to replay webhook events, try again later.";
            }

            return RedirectToAction(MVC.WebhookEvent.Index(eventType, pageSize, pageNumber, eventState, from?.ToString(WebhookEventIndexModel.DateFormat),
                    to?.ToString(WebhookEventIndexModel.DateFormat)));
        }

        private void SetIndexModelPageFields(WebhookEventIndexModel model)
        {
            model.PageTitle = "Management - Webhook events";
            model.PageNavigationUrl = Url.Action(MVC.Management.ActionNames.Index, MVC.Management.Name);
            model.PageNavigationUrlText = PageConst.Title_NavigateToManagementPage;
        }

        private void ValidateInput(DateTime? from, DateTime? to, out bool inputIsValid, out WebhookEventIndexModel inputModel)
        {
            inputIsValid = true;
            inputModel = null;

            bool hasError = false;
            if (from.HasValue)
            {
                var fromToTimeSpan = (to ?? DateTime.MaxValue).Subtract(from.Value).TotalDays;
                if (fromToTimeSpan < 0 || fromToTimeSpan > 30)
                {
                    hasError = true;
                    TempData["Danger"] = "From/To timespan must be between 1 and 30 days.";
                }
            }

            if (hasError)
            {
                inputIsValid = false;

                object modelObj = TempData["WebhookEventModel"];
                if (modelObj != null)
                {
                    inputModel = (WebhookEventIndexModel)modelObj;
                    TempData["WebhookEventModel"] = inputModel;
                }
            }
        }
    }
}
