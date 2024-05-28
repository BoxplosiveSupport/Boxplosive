using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageRetailerStores
{
	public enum WeekDay
	{
		Monday = 1,
		Tuesday = 2,
		Wednesday = 3,
		Thursday = 4,
		Friday = 5,
		Saturday = 6,
		Sunday = 7
	}

	public class RetailerStoreDateModel
	{
		public int Id { get; set; }
		public int RetailerStoreId { get; set; }
		public int RetailerId { get; set; }
		public bool KeepDefault { get; set; }
		public WeekDay? Day { get; set; }
		public DateTime? Date { get; set; }
		public bool Closed { get; set; }
		public TimeSpan TimeFrom { get; set; }
		public TimeSpan TimeTill { get; set; }
		public bool Remove { get; set; }
	}
}