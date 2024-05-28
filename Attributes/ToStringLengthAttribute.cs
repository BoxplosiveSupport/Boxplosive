using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
	/// <summary>
	/// checks the length of the string representation obtained by a ToString-call
	/// </summary>
	public class ToStringLengthAttribute : StringLengthAttribute {
		public ToStringLengthAttribute(int maximumLength)
			: base(maximumLength)
		{
		}

		public override bool IsValid(object value)
		{
			return base.IsValid(value?.ToString());
		}
	}
}