using nl.boxplosive.Service.ServiceModel.Customer;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class ReclamationSearchValueModel
    {
        public ReclamationSearchValueModel()
        {
        }

        public ReclamationSearchValueModel(int customerId, CustomerDetailsType searchType, string searchTerm, string selectedTab = null)
        {
            CustomerId = customerId;
            SearchType = searchType;
            SearchTerm = searchTerm;
            SelectedTab = selectedTab;
        }

        public int CustomerId { get; set; }
        public CustomerDetailsType SearchType { get; set; }
        public string SearchTerm { get; set; }
        public string SelectedTab { get; set; }
    }
}