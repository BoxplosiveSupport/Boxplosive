using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
	public class UiMessage
	{
		public Enums.MessageType Type { get; }

		public string Message { get; }

		public UiMessage(Enums.MessageType type, string message)
		{
			Type = type;
			Message = message;
		}

		/// <summary>
		/// Set the message in the TempData to show in the UI
		/// </summary>
		/// <param name="tempData">The TempData dictionary.</param>
		public void SetMessageInTempData(TempDataDictionary tempData)
		{
			tempData[Type.ToString()] = Message;
		}
	}
}