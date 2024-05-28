using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Drafts;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
	public abstract class DraftableModelBase<TLive, TDraft>: ViewModelBase, IDraftList<TDraft> where TDraft: IDraft where TLive: ILive
	{
		public IList<TLive> LiveObjects { get; set; }
		public IList<TDraft> Drafts { get; set; }
		public DraftableTab Tab { get; set; }

		protected void Initialize(IList<TLive> liveObjects, IList<TDraft> drafts)
		{
			LiveObjects = liveObjects;
			Drafts = drafts;
		}

		/// <summary>
		/// Returns true if there is any draft record present
		/// </summary>
		public bool SomeDraftRecordsPresent => Drafts.Any();

		/// <summary>
		/// Return true if for each live object there is a draft.
		/// </summary>
		public bool AllDraftRecordsPresent
		{
			get
			{
				int i, n = Drafts.Count();

				if (n != LiveObjects.Count)
				{
					return false;
				}

				return !LiveObjects.Select(l => l.Id).Except(Drafts.Select(c => c.GetLiveId())).Any();
			}
		}

		/// <summary>
		/// Returns true if there is any draft record editable
		/// </summary>
		public bool SomeDraftRecordsEditable => Drafts.Any(c => c.DraftStatus.IsEditable());

		/// <summary>
		/// Returns true if all draft records are editable
		/// </summary>
		public bool AllDraftRecordsEditable => Drafts.All(c => c.DraftStatus.IsEditable());

		/// <summary>
		/// Returns true if any draft record is ready
		/// </summary>
		public bool SomeDraftRecordsReady => Drafts.Any(c => c.DraftStatus.IsReady());

		/// <summary>
		/// Returns true if all draft records are ready
		/// </summary>
		public bool AllDraftRecordsReady => Drafts.All(c => c.DraftStatus.IsReady());

		public abstract bool UserHasApprovePermission { get; }

		public abstract bool UserHasEditPermission { get; }

		public Modal ConfirmDeleteModal = new Modal("confirm-delete-modal")
		{
			Title = "Delete draft",
			Question = "Are you sure you want to delete the draft?"
		};

		public void Edit()
		{
			foreach (TLive record in LiveObjects)
			{
				if (Drafts.Any(d => d.GetLiveId() == record.Id))
				{
					continue;
				}

				var draft = (TDraft)record.CreateDraft();
				draft.Save();
				Drafts.Add(draft);
			}
		}

		public void Save()
		{
			foreach (TDraft item in Drafts)
			{
				item.Save();
			}
		}

		public void Approve()
		{
			foreach (TDraft item in Drafts)
			{
				item.Approve();
			}
		}

		public void Delete()
		{
			foreach (TDraft item in Drafts)
			{
				item.Delete();
			}
		}

		public void Ready()
		{
			foreach (TDraft item in Drafts)
			{
				if (item.DraftStatus != DraftStatus.Ready)
				{
					item.Ready();
				}
			}
		}

		public void Decline()
		{
			foreach (TDraft item in Drafts)
			{
				if (item.DraftStatus == DraftStatus.Ready)
				{
					item.Decline();
				}
			}
		}
	}
}