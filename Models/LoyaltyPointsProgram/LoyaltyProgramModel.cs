using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.LoyaltyPoints;
using System;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyPointsProgram
{
    public class LoyaltyProgramModel : DraftableModelBase<boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram, LoyaltyPointsProgramDraft>
    {
        protected ILoyaltyPointsApi LoyaltyPointsProgramApi = BusinessApiFactory.GetInstance().BusinessApi<ILoyaltyPointsApi>();

        public boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram LiveObject
        {
            get { return LiveObjects.Single(); }
            set
            {
                LiveObjects = new List<boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram>
                {
                    value
                };
            }
        }

        public LoyaltyPointsProgramDraftModel Draft
        {
            get { return new LoyaltyPointsProgramDraftModel(Drafts.SingleOrDefault()); }
            set
            {
                Drafts = new List<LoyaltyPointsProgramDraft>
                {
                    value
                };
            }
        }

        public int LoyaltyProgramId { get; set; }

        public LoyaltyProgramModel()
        {
            LiveObjects = new List<boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram>();
            Drafts = new List<LoyaltyPointsProgramDraft>();
        }

        public LoyaltyProgramModel(int programId, DraftableTab tab, string navigationUrl)
        {
            LoyaltyProgramId = programId;
            Tab = tab;

            Tuple<IList<boxplosive.Business.Sdk.Entities.LoyaltyPointsProgram>, IList<LoyaltyPointsProgramDraft>> draftableProgram =
                LoyaltyPointsProgramApi.GetLoyaltyPointsProgramConfiguration(programId);

            Initialize(draftableProgram.Item1, draftableProgram.Item2);

            PageTitle = "Share loyalty points - " + LoyaltyProgramId;
            PageNavigationUrl = navigationUrl;
            PageNavigationUrlText = $"{PageConst.Title_NavigateToManagementPage} - Publications";
        }

        public override bool UserHasApprovePermission => LoyaltyProgramPermission.Instance.HasApproveRole();

        public override bool UserHasEditPermission => LoyaltyProgramPermission.Instance.HasEditRole();
    }
}