using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class ExternalRewardSupplierFactory : BaseFactory
    {
        private static readonly string _SelectList_DefaultOption = "Choose Reward supplier";

        public static List<SelectListItem> CreateSelectList(int? selected)
        {
            var results = new List<SelectListItem>();

            // Add default option
            results.Add(CreateSelectListItem(string.Empty, _SelectList_DefaultOption));
            
            // Add other options
            var suppliers = ExternalRewardSupplierRepository.GetAll();
            results.AddRange(suppliers.OrderBy(item => item.Name).Select(item => CreateSelectListItem(item.Id.ToString(), item.Name, item.Id == selected)));

            return results;
        }

        private static IExternalRewardSupplierRepository ExternalRewardSupplierRepository => DataRepositoryFactory.GetInstance().DataRepository<IExternalRewardSupplierRepository>();
    }
}