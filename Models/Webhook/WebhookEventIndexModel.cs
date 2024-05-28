using nl.boxplosive.Configuration;
using nl.boxplosive.Data.MsSql;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Webhook.Models;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Models.Webhook
{
    public class WebhookEventIndexModel : ViewModelBase
    {
        public static readonly string DateFormat = "yyyy-MM-dd";
        public static readonly int ReplayEventLimitCount = 10000;

        private static readonly string _Column_EventType = "Type";
        private static readonly string _Column_State = "State";
        private static readonly string _Column_PostedDate = "Posted date";
        private static readonly string _Column_HttpResponseCode = "HTTP code";
        private static readonly string _Column_HttpResponseErrorMessage = "Response message";
        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = 25;

        public string Column_EventType => _Column_EventType;
        public string Column_State => _Column_State;
        public string Column_PostedDate => _Column_PostedDate;
        public string Column_HttpResponseCode => _Column_HttpResponseCode;
        public string Column_HttpResponseErrorMessage => _Column_HttpResponseErrorMessage;

        private static readonly string _TenantId;

        static WebhookEventIndexModel()
        {
            _TenantId = AppConfig.Settings.GetTenantId();
        }

        public IPagedList<WebhookEventModel> PagedList { get; set; }

        public string EventType { get; set; }
        public IList<SelectListItem> EventTypes { get; set; }

        public int PageSize { get; set; }
        public IList<SelectListItem> PageSizes { get; set; }

        public string EventState { get; set; }
        public IList<SelectListItem> EventStates { get; set; }

        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public HttpStatusCode WebhookEventsGetResult_StatusCode { get; set; }
        public bool WebhookEventsGetResult_IsSuccess => ((int)WebhookEventsGetResult_StatusCode >= 200 && (int)WebhookEventsGetResult_StatusCode <= 299);

        public WebhookEventIndexModel(string eventType, int? pageSize, int? pageNumber, string eventState, DateTime? from, DateTime? to, bool returnEmptyModel)
        {
            EventType = Enum.TryParse(eventType, ignoreCase: true, out WebhookEventType eventTypeEnum)
                ? eventTypeEnum.ToString()
                : WebhookEventType.BalanceMutationCompleted.ToString();
            EventTypes = new List<SelectListItem> {
                new SelectListItem {
                    Value = WebhookEventType.BalanceMutationCompleted.ToString(),
                    Text = WebhookEventType.BalanceMutationCompleted.ToString(),
                    Selected = EventType == WebhookEventType.BalanceMutationCompleted.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventType.CouponActivated.ToString(),
                    Text = WebhookEventType.CouponActivated.ToString(),
                    Selected = EventType == WebhookEventType.CouponActivated.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventType.CouponUsed.ToString(),
                    Text = WebhookEventType.CouponUsed.ToString(),
                    Selected = EventType == WebhookEventType.CouponUsed.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventType.CouponClosed.ToString(),
                    Text = WebhookEventType.CouponClosed.ToString(),
                    Selected = EventType == WebhookEventType.CouponClosed.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventType.CouponExpired.ToString(),
                    Text = WebhookEventType.CouponExpired.ToString(),
                    Selected = EventType == WebhookEventType.CouponExpired.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventType.TransactionCompleted.ToString(),
                    Text = WebhookEventType.TransactionCompleted.ToString(),
                    Selected = EventType == WebhookEventType.TransactionCompleted.ToString()
                },
            };

            PageSize = (pageSize ?? 0) >= 1 ? pageSize.Value : _DefaultPageSize;
            PageSizes = new List<SelectListItem> {
                new SelectListItem { Value = _DefaultPageSize.ToString(), Text = _DefaultPageSize.ToString(), Selected = PageSize == _DefaultPageSize },
                new SelectListItem { Value = 50.ToString(), Text = 50.ToString(), Selected = PageSize == 50 },
                new SelectListItem { Value = 100.ToString(), Text = 100.ToString(), Selected = PageSize == 100 },
                new SelectListItem { Value = 200.ToString(), Text = 200.ToString(), Selected = PageSize == 200 },
            };
            // Force apply valid page size if current value isn't
            if (!PageSizes.Any(item => item.Value.Equals(PageSize.ToString(), StringComparison.OrdinalIgnoreCase)))
            {
                PageSize = _DefaultPageSize;
                PageSizes.First(item => item.Value.Equals(PageSize.ToString(), StringComparison.OrdinalIgnoreCase)).Selected = true;
            }

            EventState = Enum.TryParse(eventState, ignoreCase: true, out WebhookEventState eventStateEnum)
                ? eventStateEnum.ToString()
                : WebhookEventState.All.ToString();
            EventStates = new List<SelectListItem> {
                new SelectListItem {
                    Value = WebhookEventState.All.ToString(),
                    Text = WebhookEventState.All.ToString(),
                    Selected = EventState == WebhookEventState.All.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventState.Failed.ToString(),
                    Text = WebhookEventState.Failed.ToString(),
                    Selected = EventState == WebhookEventState.Failed.ToString()
                },
                new SelectListItem {
                    Value = WebhookEventState.Succeeded.ToString(),
                    Text = WebhookEventState.Succeeded.ToString(),
                    Selected = EventState == WebhookEventState.Succeeded.ToString()
                },
            };

            From = from.HasValue ? from.Value.ToLocalTimeInOperatingTimeZone() : OperatingDateTime.BeginOfDayLocal();
            To = to.HasValue ? to.Value.ToLocalTimeInOperatingTimeZone() : OperatingDateTime.EndOfDayLocal();

            int pageNumberValue = (pageNumber ?? 0) >= 1 ? pageNumber.Value : _DefaultPageNumber;
            if (returnEmptyModel)
            {
                PagedList = new StaticPagedList<WebhookEventModel>(subset: new List<WebhookEventModel>(), pageNumber: pageNumberValue, pageSize: PageSize,
                        totalItemCount: 0);

                return;
            }

            var webhookEventsGetInput = new WebhookEventsGetInput(requestId: Guid.NewGuid().ToString(), tenantId: _TenantId,
                    eventType: EventType, limit: PageSize, offset: (pageNumberValue - 1) * PageSize, state: EventState,
                    from: From.ToUtcInOperatingTimeZone(), to: To.ToUtcInOperatingTimeZone(), order: null);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var webhookWebClient = WebClientManager.WebhookWebClient;
            WebhookEventsGetResult webhookEventsGetResult = Task.Run(() => webhookWebClient.GetWebhookEventsAsync(webhookEventsGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            WebhookEventsGetResult_StatusCode = webhookEventsGetResult?.StatusCode ?? HttpStatusCode.InternalServerError;
            if (!WebhookEventsGetResult_IsSuccess)
            {
                PagedList = new StaticPagedList<WebhookEventModel>(subset: new List<WebhookEventModel>(), pageNumber: pageNumberValue, pageSize: PageSize,
                        totalItemCount: 0);

                return;
            }

            // Force apply valid page number if current value isn't
            if (webhookEventsGetResult.Data.Count == 0 && webhookEventsGetResult.Data.Offset > 0)
            {
                pageNumberValue = Math.DivRem(webhookEventsGetResult.Data.TotalCount, webhookEventsGetResult.Data.Limit, out int _) + 1;
                webhookEventsGetInput.Offset = (pageNumberValue - 1) * PageSize;
                // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
                webhookEventsGetResult = Task.Run(() => webhookWebClient.GetWebhookEventsAsync(webhookEventsGetInput)).GetAwaiter().GetResult();
                //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
                //{
                //    return await balanceWebClient.GetBalanceAsync(webModel);
                //}).Result;

                WebhookEventsGetResult_StatusCode = webhookEventsGetResult?.StatusCode ?? HttpStatusCode.InternalServerError;
                if (!WebhookEventsGetResult_IsSuccess)
                {
                    PagedList = new StaticPagedList<WebhookEventModel>(subset: new List<WebhookEventModel>(), pageNumber: pageNumberValue, pageSize: PageSize,
                            totalItemCount: 0);

                    return;
                }
            }

            var webhookEventModels = webhookEventsGetResult.Data.Items.Select(item => new WebhookEventModel(item)).ToList();
            PagedList = new StaticPagedList<WebhookEventModel>(webhookEventModels,
                    pageNumber: Math.DivRem(webhookEventsGetResult.Data.Offset, webhookEventsGetResult.Data.Limit, out int remainder) + 1,
                    pageSize: webhookEventsGetResult.Data.Limit, totalItemCount: webhookEventsGetResult.Data.TotalCount);
        }

        public RouteValueDictionary GetPagerRouteValues(int page)
        {
            return GetRouteValues(EventType, PagedList.PageSize, page, EventState, From, To);
        }

        public RouteValueDictionary GetSearchRouteValues()
        {
            return GetRouteValues(EventType, PagedList.PageSize, PagedList.PageNumber, EventState, From, To);
        }

        private RouteValueDictionary GetRouteValues(string eventType, int? pageSize, int? pageNumber, string eventState, DateTime? from, DateTime? to)
        {
            // Keep order the same as TemplateController Index GET method params
            var result = new RouteValueDictionary();
            if (!eventState.Equals(WebhookEventType.BalanceMutationCompleted.ToString(), StringComparison.OrdinalIgnoreCase))
                result.Add(MVC.WebhookEvent.IndexParams.eventType, eventType);
            if (pageSize != _DefaultPageSize)
                result.Add(MVC.WebhookEvent.IndexParams.pageSize, pageSize);
            if (pageNumber != _DefaultPageNumber)
                result.Add(MVC.WebhookEvent.IndexParams.pageNumber, pageNumber);
            if (!eventState.Equals(WebhookEventState.All.ToString(), StringComparison.OrdinalIgnoreCase))
                result.Add(MVC.WebhookEvent.IndexParams.eventState, eventState);
            if (from.HasValue && DateTime.Compare(from.Value, OperatingDateTime.BeginOfDayLocal()) != 0)
                result.Add(MVC.WebhookEvent.IndexParams.from, from?.ToString(DateFormat));
            if (to.HasValue && DateTime.Compare(to.Value, OperatingDateTime.EndOfDayLocal()) != 0)
                result.Add(MVC.WebhookEvent.IndexParams.to, to?.ToString(DateFormat));

            return result;
        }
    }
}