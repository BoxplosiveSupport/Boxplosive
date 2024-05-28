using System;
using System.Globalization;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Binders
{
    public class DateTimeModelBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            object result = base.BindModel(controllerContext, bindingContext);
            if (result == null)
            {
                return result;
            }

            return DateTime.SpecifyKind(((DateTime)result), DateTimeKind.Local);
        }
    }
}