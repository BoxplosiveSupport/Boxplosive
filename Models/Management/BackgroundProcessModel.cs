using System;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Business.Sdk.Entities;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
	public class BackgroundProcessModel
	{
		public BackgroundProcessModel(CronJob cronjob, UrlHelper url)
		{
			if (cronjob != null)
			{
				// Strip down the name
				// 1. Only use the last part, stripping out the namespace
				// 2. Strip off "CronJob", "Job" (or leave name intact).
				// 3. Add white spaces before every capital
				Name = cronjob.Name.Split('.').Last();
				Name = (Name.ToLower().Contains("cronjob") ? Name.Substring(0, Name.IndexOf("cronjob", StringComparison.InvariantCultureIgnoreCase)) : Name);
				Name = (Name.ToLower().Contains("job") ? Name.Substring(0, Name.IndexOf("job", StringComparison.InvariantCultureIgnoreCase)) : Name);
				Name = GenericHelper.AddSpacesToSentence(Name);

				LastRun = cronjob.UpdatedAt;
				Status = cronjob.Result.ToString();
			}
		}

		public string Name { get; set; }

		public DateTime LastRun { get; set; }

		public string Status { get; set; }
	}
}