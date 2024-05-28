using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.Sdk;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class TemplateIndexModel : ViewModelBase
    {
        private static readonly string _Column_Title = "Title";
        private static readonly string _Column_Type = "Type";
        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = 10;
        private static readonly string _SortColumn_Default = _Column_Title;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Title => _Column_Title;
        public string Column_Type => _Column_Type;

        public string SearchColumn { get; set; }
        public string SearchValue { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<DtoTemplate> PagedList { get; set; }

        public IList<SelectListItem> SearchColumns { get; set; }

        public TemplateIndexModel()
        {
        }

        public TemplateIndexModel(int? pageNo, int? pageSize, string searchColumn, string searchValue, string sortColumn, string sortOrder)
            : this()
        {
            int pageNoValue = (pageNo ?? 0) >= 1 ? pageNo.Value : _DefaultPageNumber;
            int pageSizeValue = (pageSize ?? 0) >= 1 ? pageSize.Value : _DefaultPageSize;
            sortColumn = sortColumn ?? _SortColumn_Default;

            SearchColumn = searchColumn;
            SearchValue = searchValue;
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            IList<DtoTemplate> templates = TemplateRepository.GetByFilterSortPage(searchColumn, searchValue, sortColumn, sortOrder, pageNoValue, pageSizeValue,
                out int outPageNo, out int outPageSize, out int outTotalRow);
            PagedList = new StaticPagedList<DtoTemplate>(templates, outPageNo, outPageSize, outTotalRow);

            SearchColumns = new List<SelectListItem> {
                new SelectListItem { Value = _Column_Title, Text = _Column_Title, Selected = searchColumn == _Column_Title },
                new SelectListItem { Value = _Column_Type, Text = _Column_Type, Selected = searchColumn == _Column_Type }
            };
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
            // Keep order the same as TemplateController Index GET method params
            var result = new RouteValueDictionary();
            result.Add(MVC.AdminArea.Template.IndexParams.page, page);

            if (addSearchRouteValues)
            {
                result.Add(MVC.AdminArea.Template.IndexParams.searchColumn, searchColumn);
                result.Add(MVC.AdminArea.Template.IndexParams.searchValue, searchValue);
            }

            result.Add(MVC.AdminArea.Template.IndexParams.sortColumn, sortColumn);
            result.Add(MVC.AdminArea.Template.IndexParams.sortOrder, sortOrder);

            return result;
        }

        private string GetSortOrderValue(string sortColumn)
        {
            return sortColumn == SortColumn ? (SortOrder == _SortOrder_Desc ? _SortOrder_Asc : _SortOrder_Desc) : _SortOrder_Asc;
        }
    }
}