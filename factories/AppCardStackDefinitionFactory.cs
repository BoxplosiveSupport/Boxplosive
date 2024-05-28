using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.AppCardStackDefinition;
using nl.boxplosive.Data.Sdk;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class AppCardStackDefinitionFactory : BaseFactory
    {
        public const string ItemPageTitle = "AppCard stack";
        public const string ListPageTitle = "AppCard stacks";
        public const string ManageText = "Manage AppCard stacks";

        public AppCardStackDefinitionModel CreateDetailPageModel(IDtoAppCardStackDefinition item, UrlHelper urlHelper, int? page = null)
        {
            var result = new AppCardStackDefinitionModel(item);
            SetPageNavigationValuesForDetailPage(result, urlHelper, page);

            result.SetPageFields(page, ItemPageTitle, urlHelper.Action(MVC.AppCardStackDefinition.ActionNames.Index), PageConst.Title_NavigateToPreviousPage);

            return result;
        }

        public AppCardStackDefinitionPageModel CreateListPageModel(DtoAppCardStackDefinitionPage items, UrlHelper urlHelper, int? page = null)
        {
            var result = new AppCardStackDefinitionPageModel(items);
            result.SetPageFields(page, ListPageTitle, urlHelper.Action(MVC.Home.ActionNames.Index, MVC.Home.Name), PageConst.Title_NavigateToHomePage);

            return result;
        }

        public IDtoAppCardStackDefinition ModelToBusiness(AppCardStackDefinitionModel model)
        {
            // ELP-6849: For each record the DisplayName and Order can be changed
            //   A new record can be added.
            //     Space is not aloud in Name column. Display Name is used and spaces are stripped.
            string name = string.IsNullOrWhiteSpace(model.Name) ? Regex.Replace(model.DisplayName, @"\s", "") : model.Name;

            var dto = new DtoAppCardStackDefinition
            {
                Id = model.Id,
                Version = model.Version,
                Name = name,
                Order = model.Order,
                DisplayName = model.DisplayName,
                CardSize = model.CardSize.ToString(),
            };

            return dto;
        }

        public void SetPageNavigationValuesForDetailPage(AppCardStackDefinitionModel model, UrlHelper urlHelper, int? page = null)
        {
            model.SetPageFields(page, ItemPageTitle, urlHelper.Action(MVC.AppCardStackDefinition.ActionNames.Index), PageConst.Title_NavigateToPreviousPage);
        }

        private static readonly string _SelectList_DefaultOption = "Choose AppCard stack";

        public static List<SelectListItem> AppCardStackDefinitionToSelectList(int? selected = null)
        {
            var stacks = AppCardStackDefinitionApi.GetAll();

            var results = new List<SelectListItem>();
            // Add default option
            results.Add(CreateSelectListItem(string.Empty, _SelectList_DefaultOption));
            // Add other options
            results.AddRange(stacks.OrderBy(s => s.DisplayName).Select(s => CreateSelectListItem(s.Id.ToString(), s.DisplayName, s.Id == selected)));

            return results;
        }

        internal static string GetAppCardStackDefinitionDisplayName(int id)
        {
            var stacks = AppCardStackDefinitionApi.GetAll();

            return stacks.FirstOrDefault(s => s.Id == id)?.DisplayName ?? string.Empty;
        }
    }
}