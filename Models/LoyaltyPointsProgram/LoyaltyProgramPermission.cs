using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public class LoyaltyProgramPermission : PermissionBase
    {
        public static LoyaltyProgramPermission Instance = new LoyaltyProgramPermission();

        public override IList<string> ApproveRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
        public override IList<string> EditRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
        public override IList<string> ViewRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
    }
}