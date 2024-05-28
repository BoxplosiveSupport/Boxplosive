using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Models.Language;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.Language;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageTemplate;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate
{
    public class LanguageMessageTemplateModel : VersionedViewModelBase
    {
        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public bool Active { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [DisplayName("Message group name")]
        [Required]
        public int MessageGroupId { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [DisplayName("Message group name")]
        //[Required]
        public string MessageGroupName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_MessageGroupName))
                    return _MessageGroupName;

                return MessageGroupNames.SingleOrDefault(item => int.Parse(item.Value) == MessageGroupId)?.Text;
            }
            set => _MessageGroupName = value;
        }

        private string _MessageGroupName { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [DisplayName("Delivery provider name")]
        [Required]
        public int DeliveryProviderId { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [DisplayName("Delivery provider name")]
        //[Required]
        public string DeliveryProviderName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_DeliveryProviderName))
                    return _DeliveryProviderName;

                return DeliveryProviderNames.SingleOrDefault(item => int.Parse(item.Value) == DeliveryProviderId)?.Text;
            }
            set => _DeliveryProviderName = value;
        }

        private string _DeliveryProviderName { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public string Name { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public bool Default { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public List<LanguageMessageTemplatePropertyModel> Properties { get; set; }

        protected static readonly string TenantId;

        static LanguageMessageTemplateModel()
        {
            TenantId = AppConfig.Settings.GetTenantId();
        }

        public LanguageMessageTemplateModel()
            : base()
        {
            // Temp dummy value
            Version = Encoding.UTF8.GetBytes(1.ToString());
        }

        public LanguageMessageTemplateDeleteResult SubmitDelete()
        {
            var languageMessageTemplateDeleteInput = new LanguageMessageTemplateDeleteInput(requestId: Guid.NewGuid().ToString(), TenantId, Id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageTemplateDeleteResult languageMessageTemplateDeleteResult = Task.Run(() => languageWebClient.DeleteLanguageMessageTemplateAsync(languageMessageTemplateDeleteInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageMessageTemplateDeleteResult;
        }

        public IList<SelectListItem> MessageGroupNames { get; set; }

        public void SetMessageGroupNames()
        {
            var languageMessageGroupsGetInput = new LanguageMessageGroupsGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageGroupsGetResult languageMessageGroupsGetResult = Task.Run(() => languageWebClient.GetLanguageMessageGroupsAsync(languageMessageGroupsGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            MessageGroupNames = new List<SelectListItem>();
            foreach (LanguageMessageGroupsGetResultDataItem item in languageMessageGroupsGetResult.Data.Items)
            {
                MessageGroupNames.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
            }
        }

        public IList<SelectListItem> Names { get; } = new List<SelectListItem>()
        {
            new SelectListItem() { Text = "AccountVerification", Value = "AccountVerification" }
        };

        public IList<SelectListItem> DeliveryProviderNames { get; set; }

        public void SetDeliveryProviderNames()
        {
            var languageDeliveryProvidersGetInput = new LanguageDeliveryProvidersGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeliveryProvidersGetResult languageDeliveryProvidersGetResult = Task.Run(() => languageWebClient.GetLanguageDeliveryProvidersAsync(languageDeliveryProvidersGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            DeliveryProviderNames = new List<SelectListItem>();
            foreach (LanguageDeliveryProvidersGetResultDataItem item in languageDeliveryProvidersGetResult.Data.Items)
            {
                DeliveryProviderNames.Add(new SelectListItem() { Text = item.Name, Value = item.Id.ToString() });
            }
        }

        public static IList<LanguageModel> GetLanguages()
        {
            var languagesGetInput = new LanguagesGetInput(requestId: Guid.NewGuid().ToString(), TenantId);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguagesGetResult languagesGetResult = Task.Run(() => languageWebClient.GetLanguagesAsync(languagesGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languagesGetResult.Data.Items
                .Select(item => new LanguageModel() { Id = item.Id, Active = item.Active, Culture = item.Culture, Default = item.Default })
                .OrderByDescending(item => item.Default).OrderBy(item => item.Culture).OrderBy(item => item.Id)
                .ToList();
        }

        public string MessageGroupPartialViewName => GetMessageGroupPartialViewName(MessageGroupName);

        public static string GetMessageGroupPartialViewName(string messageGroupName)
        {
            if (string.IsNullOrWhiteSpace(messageGroupName))
                messageGroupName = string.Empty;

            if (messageGroupName.Equals("Email", StringComparison.OrdinalIgnoreCase))
                return MVC.LanguageMessageTemplate.Views.MessageGroupNameEmail;

            return MVC.LanguageMessageTemplate.Views.Empty;
        }

        public static List<LanguageMessageTemplatePropertyModel> GetPropertiesByMessageTemplateGroup(string messageGroupName, ICollection<LanguageModel> languages)
        {
            var properties = new List<LanguageMessageTemplatePropertyModel>();
            foreach (LanguageModel language in languages)
            {
                if (messageGroupName.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "from",
                        Value = string.Empty,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "fromName",
                        Value = string.Empty,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "to",
                        Value = string.Empty,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "toName",
                        Value = string.Empty,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "subject",
                        Value = string.Empty,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "plainTextBody",
                        Value = string.Empty,
                        HtmlElement = HtmlElementModel.Textarea,
                    });
                    properties.Add(new LanguageMessageTemplatePropertyModel()
                    {
                        Id = -1,
                        Active = true,
                        LanguageId = language.Id,
                        LanguageName = language.Culture,
                        Name = "htmlBody",
                        Value = string.Empty,
                        HtmlElement = HtmlElementModel.TextareaAllowHtml,
                    });
                }
            }

            return properties;
        }

        public IDictionary<string, string> Properties_ValidationMessage { get; set; } = new Dictionary<string, string>();

        public void ValidatePropertiesByMessageTemplateName(out bool isValid)
        {
            if (Properties == null)
                Properties = new List<LanguageMessageTemplatePropertyModel>();

            string messageGroupName = !string.IsNullOrWhiteSpace(MessageGroupName) ? MessageGroupName : string.Empty;
            IList<LanguageModel> languages = GetLanguages();

            Properties_ValidationMessage = new Dictionary<string, string>();
            foreach (LanguageModel language in languages)
            {
                if (messageGroupName.Equals("Email", StringComparison.OrdinalIgnoreCase))
                {
                    string from = Properties.SingleOrDefault(item => item.Name.Equals("from", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string fromName = Properties.SingleOrDefault(item => item.Name.Equals("fromName", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string to = Properties.SingleOrDefault(item => item.Name.Equals("to", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string toName = Properties.SingleOrDefault(item => item.Name.Equals("toName", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string subject = Properties.SingleOrDefault(item => item.Name.Equals("subject", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string plainTextBody = Properties.SingleOrDefault(item => item.Name.Equals("plainTextBody", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.Value as string;
                    string htmlBody = Properties.SingleOrDefault(item => item.Name.Equals("htmlBody", StringComparison.OrdinalIgnoreCase) && item.LanguageId == language.Id)?.HtmlValue as string;
                    if (string.IsNullOrWhiteSpace(from))
                        Properties_ValidationMessage.Add($"from-{language.Culture}", $"The From email address [{language.Culture}] field is required.");
                    if (string.IsNullOrWhiteSpace(fromName))
                        Properties_ValidationMessage.Add($"fromName-{language.Culture}", $"The From name [{language.Culture}] field is required.");
                    if (string.IsNullOrWhiteSpace(to))
                        Properties_ValidationMessage.Add($"to-{language.Culture}", $"The To email address [{language.Culture}] is required.");
                    if (string.IsNullOrWhiteSpace(toName))
                        Properties_ValidationMessage.Add($"toName-{language.Culture}", $"The To name [{language.Culture}] field is required.");
                    if (string.IsNullOrWhiteSpace(subject))
                        Properties_ValidationMessage.Add($"subject-{language.Culture}", $"The Subject [{language.Culture}] field is required.");
                    if (string.IsNullOrWhiteSpace(plainTextBody))
                        Properties_ValidationMessage.Add($"plainTextBody-{language.Culture}", $"The Plain text body [{language.Culture}] field is required.");
                    if (string.IsNullOrWhiteSpace(htmlBody))
                        Properties_ValidationMessage.Add($"htmlBody-{language.Culture}", $"The HTML body [{language.Culture}] field is required.");
                }
            }

            isValid = !Properties_ValidationMessage.Any();
        }

        public IDictionary<string, string> GetPropertyLabelNamesByMessageTemplateName()
        {
            string messageGroupName = !string.IsNullOrWhiteSpace(MessageGroupName) ? MessageGroupName : string.Empty;

            var labelNames = new Dictionary<string, string>();
            if (messageGroupName.Equals("Email", StringComparison.OrdinalIgnoreCase))
            {
                labelNames.Add("from", "From email address");
                labelNames.Add("fromName", "From name");
                labelNames.Add("to", "To email address");
                labelNames.Add("toName", "To name");
                labelNames.Add("subject", "Subject");
                labelNames.Add("plainTextBody", "Plain text body");
                labelNames.Add("htmlBody", "HTML body");
            }

            return labelNames;
        }
    }
}