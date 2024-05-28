using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Sdk;
using PagedList;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc.Models.Segmentation
{
    public class SegmentationIndexModel : ViewModelBase
    {
        private static readonly string _Column_Name = "Name";
        private static readonly string _Column_ModifiedOn = "ModifiedOn";
        private static readonly string _Column_DeletedOn = "DeletedOn";

        private static readonly int _DefaultPageNumber = 1;
        private static readonly int _DefaultPageSize = 25;

        private static readonly string _SortColumn_Default = _Column_ModifiedOn;
        private static readonly string _SortOrder_Asc = "asc";
        private static readonly string _SortOrder_Desc = "desc";

        public string Column_Name => _Column_Name;
        public string Column_ModifiedOn => _Column_ModifiedOn;
        public string Column_DeletedOn => _Column_DeletedOn;

        public string SearchColumn { get; set; }
        public string SearchValue { get; set; }
        public string SortColumn { get; set; }
        public string SortOrder { get; set; }

        public IPagedList<DtoSegment> PagedList { get; set; }

        public IList<SelectListItem> SearchColumns { get; set; }

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal("Segment");

        public SegmentationIndexModel()
        {
        }

        public SegmentationIndexModel(int? pageNo, int? pageSize, string searchColumn, string searchValue, string sortColumn, string sortOrder)
            : this()
        {
            // Set defaults
            int pageNoValue = (pageNo ?? 0) >= 1 ? pageNo.Value : _DefaultPageNumber;
            int pageSizeValue = (pageSize ?? 0) >= 1 ? pageSize.Value : _DefaultPageSize;
            if (string.IsNullOrWhiteSpace(searchColumn))
                searchColumn = null;
            if (string.IsNullOrWhiteSpace(searchValue))
                searchValue = null;
            // Keep this sortOrder code before sortColumn
            if (string.IsNullOrWhiteSpace(sortOrder))
                sortOrder = string.IsNullOrWhiteSpace(sortColumn) ? _SortOrder_Desc : _SortOrder_Asc;
            if (string.IsNullOrWhiteSpace(sortColumn))
                sortColumn = _SortColumn_Default;

            SearchColumn = searchColumn;
            SearchValue = searchValue;
            SortColumn = sortColumn;
            SortOrder = sortOrder;

            SearchColumns = new List<SelectListItem> {
                new SelectListItem { Value = _Column_Name, Text = _Column_Name, Selected = !string.IsNullOrWhiteSpace(searchColumn) && searchColumn.Equals(_Column_Name, StringComparison.OrdinalIgnoreCase) },
            };

            int skip = (pageNoValue * pageSizeValue) - pageSizeValue;
            int take = pageSizeValue;
            SegmentOrderbyField? orderByField;
            if (sortColumn.Equals(_Column_Name, StringComparison.OrdinalIgnoreCase))
                orderByField = SegmentOrderbyField.Name;
            else
                orderByField = SegmentOrderbyField.LastModified;
            var orderBy = sortOrder.Equals(_SortOrder_Desc, StringComparison.OrdinalIgnoreCase) ? SegmentOrderBy.DESC : SegmentOrderBy.ASC;
            SegmentSearchValueField? searchValueField = null;
            object _searchValue = null;
            if (!string.IsNullOrWhiteSpace(searchColumn) && !string.IsNullOrWhiteSpace(searchValue))
            {
                if (searchColumn.Equals(_Column_Name, StringComparison.OrdinalIgnoreCase))
                {
                    searchValueField = SegmentSearchValueField.Name;
                    _searchValue = searchValue;
                }
            }

            (int totalCount, IList<DtoSegment> items) getSegmentsResult =
                SegmentationRepository.GetSegments(skip, take, orderByField, orderBy, searchValueField, _searchValue);
            PagedList = new StaticPagedList<DtoSegment>(getSegmentsResult.items, pageNoValue, pageSizeValue, getSegmentsResult.totalCount);
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
            // Keep order the same as SegmentationController Index GET method params
            var result = new RouteValueDictionary();
            result.Add(MVC.Segmentation.IndexParams.page, page);

            if (addSearchRouteValues)
            {
                result.Add(MVC.Segmentation.IndexParams.searchColumn, searchColumn);
                result.Add(MVC.Segmentation.IndexParams.searchValue, searchValue);
            }

            result.Add(MVC.Segmentation.IndexParams.sortColumn, sortColumn);
            result.Add(MVC.Segmentation.IndexParams.sortOrder, sortOrder);

            return result;
        }

        private string GetSortOrderValue(string sortColumn)
        {
            return sortColumn == SortColumn ? (SortOrder == _SortOrder_Desc ? _SortOrder_Asc : _SortOrder_Desc) : _SortOrder_Asc;
        }
    }
}
