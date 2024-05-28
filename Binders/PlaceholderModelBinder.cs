using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Data.Sdk.Template;
using System;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Binders
{
    public class PlaceholderModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var obj = base.BindModel(controllerContext, bindingContext);

            if (bindingContext.ModelType.Equals(typeof(PlaceholderModel)))
            {
                // Handle CreateModel() --> CreateInstance() (input validation) errors
                var placeholderModel = obj as PlaceholderModel;
                if (placeholderModel != null && !string.IsNullOrEmpty(placeholderModel.InputValidationErrorMessage))
                    bindingContext.ModelState.AddModelError($"{bindingContext.ModelName}.Value", placeholderModel.InputValidationErrorMessage);
            }

            return obj;
        }

        protected override object CreateModel(ControllerContext controllerContext, ModelBindingContext bindingContext, Type modelType)
        {
            if (modelType.Equals(typeof(PlaceholderModel)))
            {
                string dataTypeValue = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName}.Placeholder.DataType")?.AttemptedValue;
                if (!string.IsNullOrEmpty(dataTypeValue) && Enum.TryParse(dataTypeValue, true, out PlaceholderDataType dataType))
                {
                    Type instantiationType;
                    switch (dataType)
                    {
                        case PlaceholderDataType.Image:
                            instantiationType = typeof(PlaceholderImageModel);
                            break;

                        case PlaceholderDataType.RichTextEditor:
                            instantiationType = typeof(PlaceholderRichTextEditorModel);
                            break;

                        case PlaceholderDataType.Text:
                            instantiationType = typeof(PlaceholderTextModel);
                            break;

                        case PlaceholderDataType.TextArea:
                            instantiationType = typeof(PlaceholderTextAreaModel);
                            break;

                        default:
                            instantiationType = typeof(PlaceholderModel);
                            break;
                    }

                    if (instantiationType != null)
                    {
                        var obj = Activator.CreateInstance(instantiationType);
                        bindingContext.ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(null, instantiationType);
                        bindingContext.ModelMetadata.Model = obj;

                        return obj;
                    }
                }
            }

            return base.CreateModel(controllerContext, bindingContext, modelType);
        }
    }
}