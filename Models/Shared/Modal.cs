using System.Runtime.InteropServices;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
	/// <summary>
	/// This class is used to fill the reusable modal
	/// </summary>
	public class Modal
	{
		private Modal() { }

		public Modal(string id)
		{
			Id = id;
		}

		/// <summary>
		/// The modal id.
		/// </summary>
		public string Id { get; }

		/// <summary>
		/// The title of the modal.
		/// </summary>
		public string Title { get; set; }

		/// <summary>
		/// The description explaining what the action will do.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// The subject that is being used by the action, e.g. an id or name of the item.
		/// </summary>
		public bool ShowSubject { get; set; }
		
		/// <summary>
		/// The confirmation question before executing the action.
		/// </summary>
		public string Question { get; set; }
	}
}
