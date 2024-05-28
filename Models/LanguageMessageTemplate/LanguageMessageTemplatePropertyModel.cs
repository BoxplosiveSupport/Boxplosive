using nl.boxplosive.BackOffice.Mvc.Enums;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate
{
    public class LanguageMessageTemplatePropertyModel
    {
        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public int Id { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public bool Active { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public int LanguageId { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public string LanguageName { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public string Name { get; set; }

        /// <remarks>
        /// Required-if: HtmlValue is not set.
        /// </remarks>
        // [Required] --> Is manually validated in controller
        public string Value { get; set; }

        /// <remarks>
        /// Optional.
        /// </remarks>
        [AllowHtml]
        [DataType(DataType.MultilineText)]
        public string HtmlValue { get; set; }

        /// <summary>
        /// Optional.
        /// </summary>
        public HtmlElementModel? HtmlElement { get; set; }
    }
}
