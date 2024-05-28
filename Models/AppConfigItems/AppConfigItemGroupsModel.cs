using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk.AppConfigItem;
using nl.boxplosive.Business.Sdk.Entities;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Models.AppConfigItems
{
    public class AppConfigItemGroupsModel : ViewModelBase
    {
        public AppConfigItemGroupsModel()
        {
            AppConfigItemGroups = new List<string>();
        }

        public AppConfigItemGroupsModel(AppConfigItemGroups appConfigItemGroups)
        {
            AppConfigItemGroups = appConfigItemGroups.Select(l => l.Group).ToList();
        }

        [DisplayName("Settings")]
        public IList<string> AppConfigItemGroups { get; }
    }
}