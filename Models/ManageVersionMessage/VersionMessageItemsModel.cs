using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageVersionMessage
{
	public class VersionMessageItemsModel : ViewModelBase
	{
		public List<VersionMessageModel> VersionMessageItems { get; set; }
	}
}