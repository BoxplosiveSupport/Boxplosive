using System;
using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.AppConfigItem;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
	public class AppConfigItemGroupModel : DraftableModelBase<AppConfigItemBso, AppConfigItemDraftBso>
	{
		protected IAppConfigItemApi AppConfigItemApi => BusinessApiFactory.GetInstance().BusinessApi<IAppConfigItemApi>();

		public string Group { get; set; }

		public AppConfigItemGroupModel()
		{
			LiveObjects = new List<AppConfigItemBso>();
			Drafts = new List<AppConfigItemDraftBso>();
		}

		public AppConfigItemGroupModel(string group, DraftableTab tab)
		{
			Group = group;
			Tab = tab;

			Tuple<IList<IAppConfigItemBso>, IList<IAppConfigItemDraftBso>> items = AppConfigItemApi.GetGroup(group);

			Initialize(items.Item1.Select(i => (AppConfigItemBso)i).ToList(), items.Item2.Select(i => (AppConfigItemDraftBso)i).ToList());
		}

		public override bool UserHasApprovePermission => AppConfigItemGroupPermission.Instance.HasApproveRole();

		public override bool UserHasEditPermission => AppConfigItemGroupPermission.Instance.HasEditRole();
	}
}