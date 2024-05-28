using System;
using System.Globalization;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Binders
{
    public class DoubleModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var valueResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            if (string.IsNullOrEmpty(valueResult.AttemptedValue))
            {
                return 0m;
            }
            var modelState = new ModelState { Value = valueResult };
            object actualValue = null;
            try
            {
                actualValue = Convert.ToDouble(
                    valueResult.AttemptedValue.Replace(",", "."),
                    CultureInfo.InvariantCulture
                );
            }
            catch (FormatException e)
            {
                modelState.Errors.Add(e);
            }

            bindingContext.ModelState.Add(bindingContext.ModelName, modelState);
            return actualValue;
        }  
    }
}