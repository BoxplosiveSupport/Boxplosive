using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Utils;
using nl.boxplosive.Data.Sdk;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class IssuerIndexModel : ViewModelBase
    {
        private static readonly string _Column_Email = "Email";
        private static readonly string _Column_LoginName = "LoginName";
        private static readonly string _Column_Name = "Name";
        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = 10;
        private static readonly string _SortColumn_Default = _Column_Email;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Email => _Column_Email;
        public string Column_LoginName => _Column_LoginName;
        public string Column_Name => _Column_Name;

        public string SearchColumn { get; set; }
        public string SearchValue { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<IDtoIssuer> PagedList { get; set; }

        public IList<SelectListItem> SearchColumns { get; set; }

        public IssuerIndexModel()
        {
        }

        public IssuerIndexModel(bool excludeAdminAccounts, int? pageNo, int? pageSize, string searchColumn, string searchValue, string sortColumn, string sortOrder)
            : this()
        {
            int pageNoValue = (pageNo ?? 0) >= 1 ? pageNo.Value : _DefaultPageNumber;
            int pageSizeValue = (pageSize ?? 0) >= 1 ? pageSize.Value : _DefaultPageSize;
            sortColumn = sortColumn ?? _SortColumn_Default;

            SearchColumn = searchColumn;
            SearchValue = searchValue;
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            // ELP-8052: Only admin issuers (PermissionGroup=AdminIssuer) are allowed to see admin accounts
            IList<IDtoIssuer> issuers = IssuerRepository.GetByFilterSortPage(excludeAdminAccounts, searchColumn, searchValue, sortColumn, sortOrder, pageNoValue, pageSizeValue,
                out int outPageNo, out int outPageSize, out int outTotalRow);
            PagedList = new StaticPagedList<IDtoIssuer>(issuers, outPageNo, outPageSize, outTotalRow);

            SearchColumns = new List<SelectListItem>();
            SearchColumns.Add(new SelectListItem { Value = _Column_Email, Text = _Column_Email, Selected = searchColumn == _Column_Email });
            if (!Globals.B2C_Enabled)
                SearchColumns.Add(new SelectListItem { Value = _Column_LoginName, Text = "Login name", Selected = searchColumn == _Column_LoginName });
            SearchColumns.Add(new SelectListItem { Value = _Column_Name, Text = _Column_Name, Selected = searchColumn == _Column_Name });
        }

        public RouteValueDictionary GetPagerRouteValues(int page)
        {
            return GetRouteValues(page, SearchColumn, SearchValue, SortColumn, SortOrder);
        }

        public RouteValueDictionary GetSearchRouteValues()
        {
            string searchColumn = null;
            string searchValue = null;
            return GetRouteValues(PagedList.PageNumber, searchColumn, searchValue, SortColumn, SortOrder, addSearchRouteValues: false);
        }

        public RouteValueDictionary GetSortRouteValues(string sortColumn)
        {
            string sortOrder = GetSortOrderValue(sortColumn);
            return GetRouteValues(PagedList.PageNumber, SearchColumn, SearchValue, sortColumn, sortOrder);
        }

        private RouteValueDictionary GetRouteValues(int? page, string searchColumn, string searchValue, string sortColumn, string sortOrder,
            bool addSearchRouteValues = true)
        {
            // Keep order the same as IssuerController Index GET method params
            var result = new RouteValueDictionary();
            result.Add(MVC.AdminArea.Issuer.IndexParams.page, page);

            if (addSearchRouteValues)
            {
                result.Add(MVC.AdminArea.Issuer.IndexParams.searchColumn, searchColumn);
                result.Add(MVC.AdminArea.Issuer.IndexParams.searchValue, searchValue);
            }

            result.Add(MVC.AdminArea.Issuer.IndexParams.sortColumn, sortColumn);
            result.Add(MVC.AdminArea.Issuer.IndexParams.sortOrder, sortOrder);

            return result;
        }

        private string GetSortOrderValue(string sortColumn)
        {
            return sortColumn == SortColumn ? (SortOrder == _SortOrder_Desc ? _SortOrder_Asc : _SortOrder_Desc) : _SortOrder_Asc;
        }
    }
}