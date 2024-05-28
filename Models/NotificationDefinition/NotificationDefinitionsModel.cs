using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Data.Sdk;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Models.NotificationDefinition
{
    public class NotificationDefinitionsModel : ViewModelBase
    {
        public NotificationDefinitionsModel()
        {
        }

        public NotificationDefinitionsModel(DtoNotificationDefinitionPage notificationDefinitions)
        {
            Definitions = notificationDefinitions.Select(l => new NotificationDefinitionModel(l)).ToList();
            PagedList = new StaticPagedList<NotificationDefinitionModel>(Definitions, notificationDefinitions);
        }

        public IList<NotificationDefinitionModel> Definitions { get; }

        public IPagedList<NotificationDefinitionModel> PagedList { get; set; }

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal(NotificationDefinitionFactory.ItemPageTitle);
    }
}