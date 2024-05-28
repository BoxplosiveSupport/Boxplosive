using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageDeliveryProvider
{
    public class LanguageDeliveryProviderModel : VersionedViewModelBase
    {
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
        [Required]
        public List<LanguageDeliveryProviderPropertyModel> Properties { get; set; }


        public LanguageDeliveryProviderModel()
            : base()
        {
            // Temp dummy value
            Version = Encoding.UTF8.GetBytes(1.ToString());
        }

        public LanguageDeliveryProviderDeleteResult SubmitDelete(string tenantId)
        {
            var languageDeliveryProviderDeleteInput = new LanguageDeliveryProviderDeleteInput(requestId: Guid.NewGuid().ToString(), tenantId, Id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeliveryProviderDeleteResult languageDeliveryProviderDeleteResult = Task.Run(() => languageWebClient.DeleteLanguageDeliveryProviderAsync(languageDeliveryProviderDeleteInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageDeliveryProviderDeleteResult;
        }

        public string NamePartialViewName => GetNamePartialViewName(Name);

        public static string GetNamePartialViewName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                name = string.Empty;

            if (name.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
                return MVC.LanguageDeliveryProvider.Views.NameSendGrid;

            return MVC.LanguageDeliveryProvider.Views.Empty;
        }

        public IList<SelectListItem> Names { get; } = new List<SelectListItem>()
        {
            new SelectListItem() { Text = "SendGrid", Value = "SendGrid" },
        };

        public static List<LanguageDeliveryProviderPropertyModel> GetPropertiesByDeliveryProviderName(string name)
        {
            var properties = new List<LanguageDeliveryProviderPropertyModel>();
            if (name.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
            {
                properties.Add(new LanguageDeliveryProviderPropertyModel()
                {
                    Id = -1,
                    Active = true,
                    Name = "apiKey",
                    Value = string.Empty
                });
                properties.Add(new LanguageDeliveryProviderPropertyModel()
                {
                    Id = -1,
                    Active = true,
                    Name = "from",
                    Value = string.Empty
                });
            }

            return properties;
        }

        public IDictionary<string, string> Properties_ValidationMessage { get; set; } = new Dictionary<string, string>();

        public void ValidatePropertiesByDeliveryProviderName(out bool isValid)
        {
            if (string.IsNullOrWhiteSpace(Name))
                Name = string.Empty;
            if (Properties == null)
                Properties = new List<LanguageDeliveryProviderPropertyModel>();

            Properties_ValidationMessage = new Dictionary<string, string>();
            if (Name.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
            {
                string apiKey = Properties.SingleOrDefault(item => item.Name.Equals("apiKey", StringComparison.OrdinalIgnoreCase))?.Value as string;
                string from = Properties.SingleOrDefault(item => item.Name.Equals("from", StringComparison.OrdinalIgnoreCase))?.Value as string;
                if (string.IsNullOrWhiteSpace(apiKey))
                    Properties_ValidationMessage.Add("apiKey", "The API key field is required.");
                if (string.IsNullOrWhiteSpace(from))
                    Properties_ValidationMessage.Add("from", "The From email address field is required.");
            }

            isValid = !Properties_ValidationMessage.Any();
        }

        public IDictionary<string, string> GetPropertyLabelNamesByDeliveryProviderName()
        {
            if (string.IsNullOrWhiteSpace(Name))
                Name = string.Empty;

            var labelNames = new Dictionary<string, string>();
            if (Name.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
            {
                labelNames.Add("apiKey", "API key");
                labelNames.Add("from", "From email address");
            }

            return labelNames;
        }
    }
}