namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class TabModelBase
    {
        public TabModelBase()
        {
            Pagination = new PaginationModel();
        }

        public PaginationModel Pagination { get; set; }
    }
}