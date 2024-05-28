using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class CampaignsModel : ViewModelBase
    {

        public CampaignsModel()
        {
            Campaigns = new List<CampaignModel>();
        }

        public List<CampaignModel> Campaigns { get; set; } 

        public int RowCount { get; set; }

        public PaginationModel Pagination { get; set; }
    }
}