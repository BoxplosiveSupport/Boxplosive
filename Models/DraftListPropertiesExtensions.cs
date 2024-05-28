using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
	public static class DraftListPropertiesExtensions
	{
		#region Buttons on the Live tab.

		/// <summary>
		/// Return the state of the Edit button given the model.
		/// </summary>
		public static ButtonState EditButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Live)
			{
				return ButtonState.Invisible;
			}

			bool userHasEditPermission = model.UserHasEditPermission;

			if (!userHasEditPermission && !model.UserHasApprovePermission)
			{
				return ButtonState.Invisible;
			}

			if (model.AllDraftRecordsPresent)
			{
				return ButtonState.Invisible;
			}

			if (!userHasEditPermission)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Enabled;
		}

		#endregion

		#region Buttons on the Staging tab.

		/// <summary>
		/// Return the state of the Approve button given the model.
		/// </summary>
		public static ButtonState ApproveButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			if (!model.UserHasApprovePermission)
			{
				return ButtonState.Invisible;
			}

			if (!model.AllDraftRecordsReady)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Enabled;
		}

		/// <summary>
		/// Return the state of the Cancel button given the model.
		/// </summary>
		public static ButtonState CancelButtonState(this IUserPermission model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			return ButtonState.Enabled;
		}

		/// <summary>
		/// Return the state of the Decline button given the model.
		/// </summary>
		public static ButtonState DeclineButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			if (!model.UserHasApprovePermission)
			{
				return ButtonState.Invisible;
			}

			if (!model.SomeDraftRecordsReady)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Enabled;
		}

		/// <summary>
		/// Return the state of the Delete draft button given the model.
		/// </summary>
		public static ButtonState DeleteDraftButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			if ((model.UserHasApprovePermission && model.AllDraftRecordsReady) || (model.UserHasEditPermission && model.AllDraftRecordsEditable))
			{
				return ButtonState.Enabled;
			}

			if (model.UserHasApprovePermission || model.UserHasEditPermission)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Invisible;
		}

		/// <summary>
		/// Return the state of the Ready button given the model.
		/// </summary>
		public static ButtonState ReadyButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			if (!model.UserHasEditPermission)
			{
				return ButtonState.Invisible;
			}

			if (!model.AllDraftRecordsPresent)
			{
				return ButtonState.Disabled;
			}

			if (model.AllDraftRecordsReady)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Enabled;
		}

		/// <summary>
		/// Return the state of the Save draft button given the model.
		/// </summary>
		public static ButtonState SaveDraftButtonState(this IDraftListProperties model, DraftableTab selectedTab)
		{
			if (selectedTab != DraftableTab.Staging)
			{
				return ButtonState.Invisible;
			}

			if (!model.UserHasEditPermission)
			{
				return ButtonState.Invisible;
			}

			if (!model.SomeDraftRecordsEditable)
			{
				return ButtonState.Disabled;
			}

			return ButtonState.Enabled;
		}

		#endregion

		#region Tabs

		/// <summary>
		/// Return true if the user is allowed to enter the live tab.
		/// </summary>
		public static bool LiveTabAllowed(this IUserPermission model)
		{
			return model.UserHasEditPermission || model.UserHasApprovePermission;
		}

		/// <summary>
		/// Return the tab state of the live tab, given the model and whether it seems to be selected.
		/// </summary>
		public static TabState LiveTabState(this IUserPermission model, bool selected)
		{
			if (selected)
			{
				return TabState.Selected;
			}

			return TabState.Enabled;
		}

		/// <summary>
		/// Return true if the user is allowed to enter the staging tab.
		/// </summary>
		public static bool StagingTabAllowed(this IUserPermission model)
		{
			return model.UserHasEditPermission || model.UserHasApprovePermission;
		}

		/// <summary>
		/// Return the tab state of the staging tab, given the model and whether it seems to be selected.
		/// </summary>
		public static TabState StagingTabState(this IDraftListProperties model, bool selected)
		{
			if (!StagingTabAllowed(model))
			{
				return TabState.Invisible;
			}

			if (!model.SomeDraftRecordsPresent)
			{
				return TabState.Invisible;
			}

			if (selected)
			{
				return TabState.Selected;
			}

			if (!model.AllDraftRecordsPresent)
			{
				return TabState.Disabled;
			}

			return TabState.Enabled;
		}

		#endregion
	}
}