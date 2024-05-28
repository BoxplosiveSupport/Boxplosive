using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using nl.boxplosive.Business.Sdk.LoyaltyPoints;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyPointsProgram
{
    public class LoyaltyPointsProgramDraftModel : LoyaltyPointsProgramDraft
    {
        protected const string TimeSpanRegEx = @"^((^$)|(^[0-9]{1,7}$)|(([0-9]{1,7})\.([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9])))$";
        protected const string TimeSpanRegExMessage = "The value must be in days or in a timespan format e.g. d.hh:mm.";

        [Display(Name = "Enable share points and stamps")]
        public override bool SharePointsEnabled { get; set; }

        [Display(Name = "Validity period (days)")]
        [RegularExpression(TimeSpanRegEx, ErrorMessage = TimeSpanRegExMessage)]
        public string SharePointsValidityPeriodText
        {
            get => SharePointsValidityPeriod?.ToCanonicalString();
            set => SharePointsValidityPeriod = value == null ? (TimeSpan?)null : TimeSpan.Parse(value);
        }

        [Display(Name = "Message subject")]
        public override string SharePointsMessageSubject { get; set; }

        [Display(Name = "Message body")]
        public override string SharePointsMessageBody { get; set; }

        [AllowHtml]
        [Display(Name = "Landing message")]
        public override HtmlEditor SharePointsLandingMessage { get; set; }

        [AllowHtml]
        [Display(Name = "Confirmation message")]
        public override HtmlEditor SharePointsConfirmationMessage { get; set; }

        public LoyaltyPointsProgramDraftModel()
        {
        }

        public LoyaltyPointsProgramDraftModel(LoyaltyPointsProgramDraft draft)
            : base(draft)
        {
        }
    }
}