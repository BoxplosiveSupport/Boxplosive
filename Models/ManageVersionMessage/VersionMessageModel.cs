using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageVersionMessage
{
    public class VersionMessageModel : ViewModelBase
    {
        public bool New { get; set; }

        /// <summary>
        /// Application version
        /// </summary>
        [Required]
        [DisplayName("App version")]
        public string AppVersion { get; set; }

        /// <summary>
        /// Version alert message
        /// </summary>
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [DisplayName("Alert message (soft-kill)")]
        public string AlertMessage { get; set; }

        /// <summary>
        /// Version error message
        /// </summary>
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [DisplayName("Error message (hard-kill)")]
        public string ErrorMessage { get; set; }
    }
}