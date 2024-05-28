using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageGroup
{
    public class LanguageMessageGroupIndexModel : ViewModelBase
    {
        private static readonly string _Column_Active = "Active";
        private static readonly string _Column_Name = "Name";

        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = int.MaxValue;

        private static readonly string _SortColumn_Default = _Column_Name;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Active => _Column_Active;
        public string Column_Name => _Column_Name;

        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<LanguageMessageGroupModel> PagedList { get; set; }

        public HttpStatusCode LanguageMessageGroupsGetResult_StatusCode { get; set; }
        public bool LanguageMessageGroupsGetResult_IsSuccess => ((int)LanguageMessageGroupsGetResult_StatusCode >= 200 && (int)LanguageMessageGroupsGetResult_StatusCode <= 299);

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal("Language message group");

        public LanguageMessageGroupIndexModel(string tenantId, string sortColumn, string sortOrder)
        {
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            var languageMessageGroupsGetInput = new LanguageMessageGroupsGetInput(requestId: Guid.NewGuid().ToString(), tenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageGroupsGetResult languageMessageGroupsGetResult = Task.Run(() => languageWebClient.GetLanguageMessageGroupsAsync(languageMessageGroupsGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            LanguageMessageGroupsGetResult_StatusCode = languageMessageGroupsGetResult?.StatusCode ?? HttpStatusCode.InternalServerError;
            if (!LanguageMessageGroupsGetResult_IsSuccess)
            {
                PagedList = new StaticPagedList<LanguageMessageGroupModel>(subset: new List<LanguageMessageGroupModel>(), pageNumber: _DefaultPageNumber, pageSize: _DefaultPageSize,
                totalItemCount: 0);
                return;
            }

            var languageMessageGroupModels = SortLanguageMessageGroupsGetResultDataItems(languageMessageGroupsGetResult.Data.Items, sortColumn, sortOrder)
                .Select(item => new LanguageMessageGroupModel()
                {
                    Id = item.Id,
                    Active = item.Active,
                    Name = item.Name,
                })
                .ToList();
            PagedList = new StaticPagedList<LanguageMessageGroupModel>(languageMessageGroupModels,
                    pageNumber: Math.DivRem(languageMessageGroupsGetResult.Data.Offset, languageMessageGroupsGetResult.Data.Limit, out int remainder) + 1,
                    pageSize: languageMessageGroupsGetResult.Data.Limit, totalItemCount: languageMessageGroupsGetResult.Data.TotalCount);
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
            // Keep order the same as LanguageMessageGroupController Index GET method params
            var result = new RouteValueDictionary();
            if (!string.IsNullOrWhiteSpace(sortColumn))
                result.Add(MVC.LanguageMessageGroup.IndexParams.sortColumn, sortColumn);
            if (!string.IsNullOrWhiteSpace(sortOrder))
                result.Add(MVC.LanguageMessageGroup.IndexParams.sortOrder, sortOrder);

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

        private IEnumerable<LanguageMessageGroupsGetResultDataItem> SortLanguageMessageGroupsGetResultDataItems(IReadOnlyCollection<LanguageMessageGroupsGetResultDataItem> items,
                string sortColumn, string sortOrder)
        {
            if (sortColumn == null)
                sortColumn = string.Empty;

            if (sortOrder == null)
                sortOrder = string.Empty;

            IEnumerable<LanguageMessageGroupsGetResultDataItem> sortedItems;
            if (sortColumn.Equals(Column_Active, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Active);
                else
                    sortedItems = items.OrderBy(item => item.Active);
            }
            else if (sortColumn.Equals(Column_Name, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Name);
                else
                    sortedItems = items.OrderBy(item => item.Name);
            }
            else
            {
                sortedItems = items.OrderBy(item => item.Name);
            }

            return sortedItems;
        }
    }
}
