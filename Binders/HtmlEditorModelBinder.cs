using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Binders
{
    public class HtmlEditorModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            if (bindingContext.ModelType == typeof(HtmlEditor))
            {
                var propertyType = bindingContext.ModelMetadata.ContainerType.GetProperty(bindingContext.ModelMetadata.PropertyName);
                object[] customAttributes = propertyType.GetCustomAttributes(true);

                bool skipValidation = customAttributes.Any(attr => attr is AllowHtmlAttribute);
                bool isRequired = customAttributes.Any(attr => attr is RequiredAttribute);

                ValueProviderResult valueProviderResult =
                    ((IUnvalidatedValueProvider) bindingContext.ValueProvider).GetValue(bindingContext.ModelName, skipValidation);
                var value = valueProviderResult.AttemptedValue;
                if (!isRequired && string.IsNullOrEmpty(value))
                {
                    return null;
                }

                HtmlEditor result = null;
                try
                {
                    result = HtmlEditor.Create(value, allowExternalImages: false);
                }
                catch (BadImageFormatException ex)
                {
                    bindingContext.ModelState.AddModelError(bindingContext.ModelName, "External images are not allowed");
                    bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);
                }

                return result;
            }

            return base.BindModel(controllerContext, bindingContext);
        }
    }
}