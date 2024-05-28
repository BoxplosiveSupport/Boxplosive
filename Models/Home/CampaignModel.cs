using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class CampaignModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string IconImgUrl { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public bool Finished { get; set; }

        public string ModifyUrl { get; set; }
        public string CopyUrl { get; set; }

        public ActionMetadata ModifyMetadata { get; set; }

        public ActionMetadata CopyMetadata { get; set; }
    }
}