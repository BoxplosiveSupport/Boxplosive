namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
	public class ModalInfo
	{
		private ModalInfo()
		{
		}

		public ModalInfo(string id)
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
	}
}
