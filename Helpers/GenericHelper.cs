using System;
using System.Diagnostics;
using System.Text;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
	public class GenericHelper
	{
		/// <summary>
		/// Add a white space before every capital character
		/// </summary>
		/// <param name="text">The text to add spaces to</param>
		/// <returns>The result after adding spaces</returns>
		public static string AddSpacesToSentence(string text)
		{
			if (string.IsNullOrWhiteSpace(text))
				return "";
			var newText = new StringBuilder(text.Length * 2);
			newText.Append(text[0]);
			for (var i = 1; i < text.Length; i++)
			{
				if (char.IsUpper(text[i]) && text[i - 1] != ' ')
					newText.Append(' ');
				newText.Append(text[i]);
			}
			return newText.ToString();
		}
	}
}