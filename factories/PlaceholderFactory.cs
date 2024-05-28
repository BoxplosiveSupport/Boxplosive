using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Template;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.Service.ServiceModel.Campaign;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public static class PlaceholderFactory
    {
        public static PlaceholderModel CreatePlaceholderModel(DtoPlaceholder placeHolder, PlaceholderValue placeHolderValue)
        {
            switch (placeHolder.DataType)
            {
                case PlaceholderDataType.Image:
                    return new PlaceholderImageModel(placeHolder, placeHolderValue);

                case PlaceholderDataType.RichTextEditor:
                    return new PlaceholderRichTextEditorModel(placeHolder, placeHolderValue);

                case PlaceholderDataType.Text:
                    return new PlaceholderTextModel(placeHolder, placeHolderValue);

                case PlaceholderDataType.TextArea:
                    return new PlaceholderTextAreaModel(placeHolder, placeHolderValue);

                default:
                    return new PlaceholderModel(placeHolder, placeHolderValue);
            }
        }

        public static IList<PlaceholderModel> GetPlaceholderModels(int? templateId, ICollection<PlaceholderValue> servicePlaceholderValues)
        {
            var result = new List<PlaceholderModel>();
            if (templateId > 0)
            {
                var template = _TemplateApi.Get(templateId.Value);
                var placeholders = template.GetPlaceholders();

                IDictionary<int, PlaceholderValue> placeholderValues = servicePlaceholderValues.ToDictionary(item => item.PlaceholderId, item => item);
                foreach (var placeholder in placeholders)
                {
                    placeholderValues.TryGetValue(placeholder.Id, out PlaceholderValue placeholderValue);
                    result.Add(CreatePlaceholderModel(placeholder, placeholderValue));
                }
            }

            return result;
        }

        #region API

        private static readonly ITemplateApi _TemplateApi = BusinessApiFactory.GetInstance().BusinessApi<ITemplateApi>();

        #endregion API
    }
}