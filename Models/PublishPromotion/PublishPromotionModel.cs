using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;

namespace nl.boxplosive.BackOffice.Mvc.Models.PublishPromotion
{
    public class PublishPromotionModel : ViewModelBase
    {
        public PromotionModel Promotion { get; set; }
    }
}