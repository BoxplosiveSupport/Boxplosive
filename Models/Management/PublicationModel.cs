using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class PublicationModel
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public DateTime? VisibleFrom { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public string Status { get; set; }

        /// <remarks>
        /// LoyaltyPointsProgram.IsPublic.
        /// </remaks>
        public bool IsPublic { get; set; }

        public string ModifyUrl { get; set; }

        public string CopyUrl { get; set; }

        public string ConfigurationUrl { get; set; }

        public string ReminderUrl { get; set; }

        public string IconImgUrl { get; set; }
    }
}