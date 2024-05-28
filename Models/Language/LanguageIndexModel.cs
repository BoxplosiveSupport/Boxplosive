using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.Language;
using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Models.Language
{
    public class LanguageIndexModel : ViewModelBase
    {
        private static readonly string _Column_Active = "Active";
        private static readonly string _Column_Culture = "Culture";
        private static readonly string _Column_Default = "Default";

        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = int.MaxValue;

        private static readonly string _SortColumn_Default = _Column_Culture;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Active => _Column_Active;
        public string Column_Culture => _Column_Culture;
        public string Column_Default => _Column_Default;

        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<LanguageModel> PagedList { get; set; }

        public HttpStatusCode LanguagesGetResult_StatusCode { get; set; }
        public bool LanguagesGetResult_IsSuccess => ((int)LanguagesGetResult_StatusCode >= 200 && (int)LanguagesGetResult_StatusCode <= 299);

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal("Language");

        public LanguageIndexModel(string tenantId, string sortColumn, string sortOrder)
        {
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            var languagesGetInput = new LanguagesGetInput(requestId: Guid.NewGuid().ToString(), tenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguagesGetResult languagesGetResult = Task.Run(() => languageWebClient.GetLanguagesAsync(languagesGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            LanguagesGetResult_StatusCode = languagesGetResult?.StatusCode ?? HttpStatusCode.InternalServerError;
            if (!LanguagesGetResult_IsSuccess)
            {
                PagedList = new StaticPagedList<LanguageModel>(subset: new List<LanguageModel>(), pageNumber: _DefaultPageNumber, pageSize: _DefaultPageSize,
                totalItemCount: 0);
                return;
            }

            var languageModels = SortLanguagesGetResultDataItems(languagesGetResult.Data.Items, sortColumn, sortOrder)
                .Select(item => new LanguageModel()
                {
                    Id = item.Id,
                    Active = item.Active,
                    Culture = item.Culture,
                    Default = item.Default,
                })
                .ToList();
            PagedList = new StaticPagedList<LanguageModel>(languageModels,
                    pageNumber: Math.DivRem(languagesGetResult.Data.Offset, languagesGetResult.Data.Limit, out int remainder) + 1,
                    pageSize: languagesGetResult.Data.Limit, totalItemCount: languagesGetResult.Data.TotalCount);
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
            // Keep order the same as LanguageController Index GET method params
            var result = new RouteValueDictionary();
            if (!string.IsNullOrWhiteSpace(sortColumn))
                result.Add(MVC.Language.IndexParams.sortColumn, sortColumn);
            if (!string.IsNullOrWhiteSpace(sortOrder))
                result.Add(MVC.Language.IndexParams.sortOrder, sortOrder);

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

        private IEnumerable<LanguagesGetResultDataItem> SortLanguagesGetResultDataItems(IReadOnlyCollection<LanguagesGetResultDataItem> items,
                string sortColumn, string sortOrder)
        {
            if (sortColumn == null)
                sortColumn = string.Empty;

            if (sortOrder == null)
                sortOrder = string.Empty;

            IEnumerable<LanguagesGetResultDataItem> sortedItems;
            if (sortColumn.Equals(Column_Active, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Active);
                else
                    sortedItems = items.OrderBy(item => item.Active);
            }
            else if (sortColumn.Equals(Column_Culture, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Culture);
                else
                    sortedItems = items.OrderBy(item => item.Culture);
            }
            else if (sortColumn.Equals(Column_Default, StringComparison.OrdinalIgnoreCase))
            {
                if (sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase))
                    sortedItems = items.OrderByDescending(item => item.Default);
                else
                    sortedItems = items.OrderBy(item => item.Default);
            }
            else
            {
                sortedItems = items.OrderBy(item => item.Culture);
            }

            return sortedItems;
        }
    }
}
