using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageTemplate;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate
{
    public class LanguageMessageTemplateIndexModel : ViewModelBase
    {
        private static readonly string _Column_Active = "Active";
        private static readonly string _Column_Default = "Default";
        private static readonly string _Column_DeliveryProvider = "Delivery provider";
        private static readonly string _Column_Name = "Name";
        private static readonly string _Column_MessageGroup = "Message group";

        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = int.MaxValue;

        private static readonly string _SortColumn_Default = _Column_Name;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Active => _Column_Active;
        public string Column_Default => _Column_Default;
        public string Column_DeliveryProvider => _Column_DeliveryProvider;
        public string Column_Name => _Column_Name;
        public string Column_MessageGroup => _Column_MessageGroup;

        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<LanguageMessageTemplateModel> PagedList { get; set; }

        public HttpStatusCode LanguageMessageTemplatesGetResult_StatusCode { get; set; }
        public bool LanguageMessageTemplatesGetResult_IsSuccess => ((int)LanguageMessageTemplatesGetResult_StatusCode >= 200 && (int)LanguageMessageTemplatesGetResult_StatusCode <= 299);

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal("Language message template");

        protected static readonly string TenantId;

        static LanguageMessageTemplateIndexModel()
        {
            TenantId = AppConfig.Settings.GetTenantId();
        }

        public LanguageMessageTemplateIndexModel(string sortColumn, string sortOrder)
        {
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            var languageMessageTemplatesGetInput = new LanguageMessageTemplatesGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageTemplatesGetResult languageMessageTemplatesGetResult = Task.Run(() => languageWebClient.GetLanguageMessageTemplatesAsync(languageMessageTemplatesGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            LanguageMessageTemplatesGetResult_StatusCode = languageMessageTemplatesGetResult?.StatusCode ?? HttpStatusCode.InternalServerError;
            if (!LanguageMessageTemplatesGetResult_IsSuccess)
            {
                PagedList = new StaticPagedList<LanguageMessageTemplateModel>(subset: new List<LanguageMessageTemplateModel>(), pageNumber: _DefaultPageNumber, pageSize: _DefaultPageSize,
                totalItemCount: 0);
                return;
            }

            var languageMessageGroupsGetInput = new LanguageMessageGroupsGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            LanguageMessageGroupsGetResult languageMessageGroupsGetResult = Task.Run(() => languageWebClient.GetLanguageMessageGroupsAsync(languageMessageGroupsGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            var languageDeliveryProvidersGetInput = new LanguageDeliveryProvidersGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            LanguageDeliveryProvidersGetResult languageDeliveryProvidersGetResult = Task.Run(() => languageWebClient.GetLanguageDeliveryProvidersAsync(languageDeliveryProvidersGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            var languageMessageTemplateModels = SortLanguageMessageTemplatesGetResultDataItems(
                languageMessageTemplatesGetResult.Data.Items.Select(item => new LanguageMessageTemplateModel()
                {
                    Id = item.Id,
                    Active = item.Active,
                    MessageGroupId = item.MessageGroupId,
                    MessageGroupName = languageMessageGroupsGetResult.Data.Items.Single(itemLmg => itemLmg.Id == item.MessageGroupId).Name,
                    DeliveryProviderId = item.DeliveryProviderId,
                    DeliveryProviderName = languageDeliveryProvidersGetResult.Data.Items.Single(itemLmg => itemLmg.Id == item.DeliveryProviderId).Name,
                    Name = item.Name,
                    Default = item.Default,
                    Properties = null, // Is not used on Index page
                }).ToList(), sortColumn, sortOrder);
            PagedList = new StaticPagedList<LanguageMessageTemplateModel>(languageMessageTemplateModels,
                    pageNumber: Math.DivRem(languageMessageTemplatesGetResult.Data.Offset, languageMessageTemplatesGetResult.Data.Limit, out int remainder) + 1,
                    pageSize: languageMessageTemplatesGetResult.Data.Limit, totalItemCount: languageMessageTemplatesGetResult.Data.TotalCount);
        }

        public RouteValueDictionary GetPagerRouteValues(int page)
        {
            return GetRouteValues(SortColumn, SortOrder);
        }

        public RouteValueDictionary GetSortRouteValues(string sortColumn)
        {
            string sortOrder = GetSortOrderValue(sortColumn);

            return GetRouteValues(sortColumn, sortOrder);
        }

        private RouteValueDictionary GetRouteValues(string sortColumn, string sortOrder)
        {
            // Keep order the same as LanguageMessageTemplateController Index GET method params
            var result = new RouteValueDictionary();
            if (!string.IsNullOrWhiteSpace(sortColumn))
                result.Add(MVC.LanguageMessageTemplate.IndexParams.sortColumn, sortColumn);
            if (!string.IsNullOrWhiteSpace(sortOrder))
                result.Add(MVC.LanguageMessageTemplate.IndexParams.sortOrder, sortOrder);

            return result;
        }

        private string GetSortOrderValue(string sortColumn)
        {
            if (sortColumn == null)
                sortColumn = string.Empty;

            return sortColumn.Equals(SortColumn, StringComparison.OrdinalIgnoreCase)
                    ? (SortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase) ? _SortOrder_Asc : _SortOrder_Desc)
                    : _SortOrder_Asc;
        }

        private IList<LanguageMessageTemplateModel> SortLanguageMessageTemplatesGetResultDataItems(IReadOnlyCollection<LanguageMessageTemplateModel> items,
                string sortColumn, string sortOrder)
        {
            if (sortColumn == null)
                sortColumn = string.Empty;

            if (sortOrder == null)
                sortOrder = string.Empty;

            IEnumerable<LanguageMessageTemplateModel> sortedItems;
            if (sortColumn.Equals(Column_Active, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Active);
                else
                    sortedItems = items.OrderBy(item => item.Active);
            }
            else if (sortColumn.Equals(Column_Default, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Default);
                else
                    sortedItems = items.OrderBy(item => item.Default);
            }
            else if (sortColumn.Equals(Column_DeliveryProvider, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.DeliveryProviderName);
                else
                    sortedItems = items.OrderBy(item => item.DeliveryProviderName);
            }
            else if (sortColumn.Equals(Column_Name, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Name);
                else
                    sortedItems = items.OrderBy(item => item.Name);
            }
            else if (sortColumn.Equals(Column_MessageGroup, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.MessageGroupName);
                else
                    sortedItems = items.OrderBy(item => item.MessageGroupName);
            }
            else
            {
                sortedItems = items.OrderBy(item => item.Name);
            }

            return sortedItems.ToList();
        }
    }
}