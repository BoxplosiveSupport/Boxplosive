using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class AnalyseModel : ViewModelBase
    {
        public CampaignsModel LiveCampaigns { get; set; }
        public CampaignsModel FinishedCampaigns { get; set; }

        public string AllLivePublicationUrl { get; set; }
        public string AllFinishedPublicationUrl { get; set; }

        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public decimal TotalDiscountGiven { get; set; }

        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public int IssuedCoupons { get; set; }
        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public int ClaimedCoupons { get; set; }
        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public int CashedCoupons { get; set; }

        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public int UsedPoints { get; set; }
        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        public int GivenPoints { get; set; }
    }
}
