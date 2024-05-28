using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public class AppConfigItemGroupPermission : PermissionBase
    {
        public static AppConfigItemGroupPermission Instance = new AppConfigItemGroupPermission();

        public override IList<string> ApproveRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
        public override IList<string> EditRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
        public override IList<string> ViewRoles { get; } = new List<string> { ApplicationManagerRole, CampaignManagerRole };
    }
}