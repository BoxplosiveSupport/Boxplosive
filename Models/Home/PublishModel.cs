using System.Collections.Generic;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class PublishModel
    {
        public PublishModel()
        {
            Drafts = new List<ProcessModule>();
            ReadyToPublish = new List<ProcessModule>();
        }

        public CampaignType CampaignType { get; set; }

        public List<ProcessModule> Drafts { get; set; }

        public List<ProcessModule> ReadyToPublish { get; set; }
    }
}