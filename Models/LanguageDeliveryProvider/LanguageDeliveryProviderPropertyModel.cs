using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageDeliveryProvider
{
    public class LanguageDeliveryProviderPropertyModel
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
        public string Name { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        // [Required] --> Is manually validated in controller
        public string Value { get; set; }
    }
}