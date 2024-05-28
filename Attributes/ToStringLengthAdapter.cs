using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
	public class ToStringLengthAdapter : DataAnnotationsModelValidator<ToStringLengthAttribute>
	{
		public ToStringLengthAdapter(ModelMetadata metadata, ControllerContext context, ToStringLengthAttribute attribute)
			: base(metadata, context, attribute)
		{
		}

		public override IEnumerable<ModelClientValidationRule> GetClientValidationRules()
		{
			return new[] { new ModelClientValidationStringLengthRule(ErrorMessage, Attribute.MinimumLength, Attribute.MaximumLength) };
		}
	}
}