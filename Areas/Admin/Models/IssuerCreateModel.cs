using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class IssuerCreateModel : IssuerModelBase
    {
        /// <remarks>
        /// Required field.
        /// </remarks>
        [Required]
        public override string Password { get; set; }

        public IssuerCreateModel()
            : base()
        {
        }
    }
}