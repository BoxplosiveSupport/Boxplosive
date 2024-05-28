using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class TemplateFactory : BaseFactory
    {
        private static readonly string _SelectList_DefaultOption = "Choose AppCard template";

        public static List<SelectListItem> CreateSelectList(TemplateType type, int? selected)
        {
            var results = new List<SelectListItem>();

            // Add default option
            results.Add(CreateSelectListItem(string.Empty, _SelectList_DefaultOption));
            // Add other options
            var templates = TemplateApi.GetAllByType(type);
            results.AddRange(templates.OrderBy(item => item.Title).Select(item => CreateSelectListItem(item.Id.ToString(), item.Title, item.Id == selected)));

            return results;
        }
    }
}