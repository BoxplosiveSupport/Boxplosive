using Newtonsoft.Json;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public class CodeActionAdditionalSettings
    {
        [JsonProperty("points")]
        public int Points { get; set; }
    }
}