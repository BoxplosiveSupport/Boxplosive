using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.NotificationDefinition;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class NotificationDefinitionFactory : BaseFactory
    {
        public const string ItemPageTitle = "Notification";
        public const string FeatureName = "Notifications";
        public const string ListPageTitle = "Notifications";
        public const string ManageText = "Manage notifications";

        public const string NotificationProviderNameUnknownText = "Choose notification type";

        private static INotificationDefinitionRepository _NotificationDefinitionRepository =>
            DataRepositoryFactory.GetInstance().DataRepository<INotificationDefinitionRepository>();

        public NotificationDefinitionModel CreateDetailPageModel(IDtoNotificationDefinition notificationDefinition, UrlHelper urlHelper, int? page = null)
        {
            var result = new NotificationDefinitionModel(notificationDefinition);
            SetPageNavigationValuesForDetailPage(result, urlHelper, page);

            result.SetPageFields(page, ItemPageTitle, urlHelper.Action(MVC.NotificationDefinition.ActionNames.Index), PageConst.Title_NavigateToPreviousPage);

            return result;
        }

        public NotificationDefinitionsModel CreateListPageModel(DtoNotificationDefinitionPage notificationDefinitions, UrlHelper urlHelper, int? page = null)
        {
            var result = new NotificationDefinitionsModel(notificationDefinitions);
            result.SetPageFields(page, ListPageTitle, urlHelper.Action(MVC.Home.ActionNames.Index, MVC.Home.Name), PageConst.Title_NavigateToHomePage);

            return result;
        }

        public DtoNotificationDefinition ModelToBusiness(NotificationDefinitionModel model)
        {
            var dto = new DtoNotificationDefinition
            {
                Id = model.Id,
                Name = model.Name,
                NotificationType = EnumHelper<NotificationProviderName>.ConvertOrDefault(model.Type.Value),
                CustomEvent = model.CustomEvent,
                Arguments = model.Arguments,
                Version = model.Version
            };

            return dto;
        }

        public static string NotificationTypeToDisplayText(NotificationType value)
        {
            switch (value)
            {
                case NotificationType.Unknown:
                    return NotificationProviderNameUnknownText;

                default:
                    return value.ToString();
            }
        }

        public void SetPageNavigationValuesForDetailPage(NotificationDefinitionModel model, UrlHelper urlHelper, int? page = null)
        {
            model.SetPageFields(page, ItemPageTitle, urlHelper.Action(MVC.NotificationDefinition.ActionNames.Index), PageConst.Title_NavigateToPreviousPage);
        }
    }
}