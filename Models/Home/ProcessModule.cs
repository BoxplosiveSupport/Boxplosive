using System;
using System.Collections.Generic;
using nl.boxplosive.Service.ProcessModel;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
	public class ProcessModule
	{
		public string Title { get; set; }
		public string IconImgUrl { get; set; }

		public string ActionUrl { get; set; }
		public string DeleteUrl { get; set; }

		public int CounterValue { get; set; }
		public string CounterDescription { get; set; }

		public string Type { get; set; }

		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }

		public DateTime ModifiedDate { get; set; }

		public CampaignProcessType ProcessType { get; set; }

		public ActionMetadata ActionMetadata { get; set; }

		public ActionMetadata DeleteMetadata { get; set; }

		public string FeatureName { get; set; }

		public IList<string> Roles { get; set; }
	}
}