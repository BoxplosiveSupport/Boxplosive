using System.Collections.Generic;
using System.Security.Principal;
using nl.boxplosive.Business.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
	public abstract class PermissionBase
	{
		public const string ApplicationManagerRole = "ApplicationManager";
		public const string CampaignManagerRole = "CampaignManager";

		public IPrincipal User => System.Web.HttpContext.Current.User;

		public abstract IList<string> ApproveRoles { get; }
		public abstract IList<string> EditRoles { get; }
		public abstract IList<string> ViewRoles { get; }

		public bool HasApproveRole()
		{
			return User != null && User.IsInOneRole(ApproveRoles);
		}

		public bool HasEditRole()
		{
			return User != null && User.IsInOneRole(EditRoles);
		}

		public bool HasViewRole()
		{
			return User != null && User.IsInOneRole(ViewRoles);
		}
	}
}