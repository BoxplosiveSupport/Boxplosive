namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public class PaginationModel
    {
	    public const int DefaultPageNumber = 1;
	    public const int DefaultPageSize = 10;

		public PaginationModel()
        {
            PageRange = 4;
        }

        private int _pageNumber;
        public int PageNumber
        {
            get { return _pageNumber < 1 ? 1 : _pageNumber; }
            set { _pageNumber = value; }
        }

        public int ItemCount { get; set; }

        public int PageSize { get; set; }

        /// <summary>
        /// Calculates the page count based on the ItemCount and the PageSize
        /// </summary>
        public int PageCount
        {
            get
            {
                var pages = ItemCount/PageSize;
                if (ItemCount > PageSize &&
                    ItemCount % PageSize > 0)
                {
                    pages++;
                }

                return pages;
            }
            
        }

        private int _pageRange;
        public int PageRange
        {
            get { return _pageRange < 1 ? 1 : _pageRange; }
            set { _pageRange = value; }
        }

        public int PaginationStart
        {
            get
            {
                var start = PageNumber - PageRange;
                if (start < 1) start = 1;
                return start;
            }
        }

        public int PaginationEnd
        {
            get
            {
                var end = PageNumber + PageRange;
                if (end > PageCount) end = PageCount;
                return end;
            }
        }
    }
}