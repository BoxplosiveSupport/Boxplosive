using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;

namespace nl.boxplosive.BackOffice.Mvc.Models.PublishLoyalty
{
    public class PublishLoyaltyModel : ViewModelBase, ILoyaltyModel
    {
        public LoyaltyModel Loyalty { get; set; }
    }
}