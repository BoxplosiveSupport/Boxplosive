using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Sdk;
using PagedList;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.AppCardStackDefinition
{
    public class AppCardStackDefinitionPageModel : ViewModelBase
    {
        public AppCardStackDefinitionPageModel()
        {
        }

        public AppCardStackDefinitionPageModel(DtoAppCardStackDefinitionPage items)
        {
            Definitions = items.Select(l => new AppCardStackDefinitionModel(l)).ToList();
            PagedList = new StaticPagedList<AppCardStackDefinitionModel>(Definitions, items);
        }

        public IList<AppCardStackDefinitionModel> Definitions { get; }

        public IPagedList<AppCardStackDefinitionModel> PagedList { get; set; }

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal(AppCardStackDefinitionFactory.ItemPageTitle);
    }
}