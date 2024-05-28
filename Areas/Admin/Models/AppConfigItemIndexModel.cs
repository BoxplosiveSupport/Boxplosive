using System.Collections.Generic;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class AppConfigItemIndexModel : ViewModelBase
    {
        public IList<IDtoAppConfigItem> Items { get; set; }

        public AppConfigItemIndexModel()
        {
            Items = AppConfigItemRepository.GetAll();
        }
    }
}