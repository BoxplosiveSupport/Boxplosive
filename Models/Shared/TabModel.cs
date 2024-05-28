namespace nl.boxplosive.BackOffice.Mvc.Models.Shared
{
	public class TabModel
	{
		public string Title { get; set; }
		public string ActionName { get; set; }
		public object RouteValues { get; set; }
		public TabState State { get; set; }
	}
}
