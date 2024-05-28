using nl.boxplosive.BackOffice.Mvc.Models;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores
{
	public class ManageRetailerStoresModel : ViewModelBase
	{
		public string SearchText { get; set; }

		public IPagedList<RetailerStoreModel> RetailerStores { get; set; }
	}
}