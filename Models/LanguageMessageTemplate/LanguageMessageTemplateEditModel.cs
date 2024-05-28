using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Models.Language;
using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageTemplate;
using NLog;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate
{
    public class LanguageMessageTemplateEditModel : LanguageMessageTemplateModel
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        [Required]
        public new int Id { get; set; }

        public LanguageMessageTemplateEditModel()
            : base()
        {
            SetMessageGroupNames();
            SetDeliveryProviderNames();
        }

        public LanguageMessageTemplateEditModel(int id)
            : this()
        {
            var languageMessageTemplateGetInput = new LanguageMessageTemplateGetInput(requestId: Guid.NewGuid().ToString(), TenantId, id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageTemplateGetResult languageMessageTemplateGetResult = Task.Run(() => languageWebClient.GetLanguageMessageTemplateAsync(languageMessageTemplateGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            if (languageMessageTemplateGetResult == null || !languageMessageTemplateGetResult.IsSuccessStatusCode || languageMessageTemplateGetResult.Data == null)
                throw new Exception($"Failed to create language message template edit model: responseStatusCode={languageMessageTemplateGetResult?.StatusCode} responseData={(languageMessageTemplateGetResult?.Data == null ? "null" : "notnull")}");

            IList<LanguageModel> languages = GetLanguages();

            string messageGroupName = MessageGroupNames.Single(item => int.Parse(item.Value) == languageMessageTemplateGetResult.Data.MessageGroupId).Text;
            List<LanguageMessageTemplatePropertyModel> messageTemplateProperties = GetPropertiesByMessageTemplateGroup(messageGroupName, languages);
            foreach (LanguageMessageTemplatesGetResultDataItem_Property messageTemplatePropertyResult in languageMessageTemplateGetResult.Data.Properties)
            {
                LanguageMessageTemplatePropertyModel messageTemplateProperty =
                    messageTemplateProperties.SingleOrDefault(item => item.LanguageId == messageTemplatePropertyResult.LanguageId && item.Name.Equals(messageTemplatePropertyResult.Name, StringComparison.OrdinalIgnoreCase));
                if (messageTemplateProperty != null)
                {
                    messageTemplateProperty.Id = messageTemplatePropertyResult.Id;
                    if (messageTemplateProperty.HtmlElement == HtmlElementModel.TextareaAllowHtml)
                        messageTemplateProperty.HtmlValue = messageTemplatePropertyResult.Value;
                    else
                        messageTemplateProperty.Value = messageTemplatePropertyResult.Value;
                }
            }

            //
            Id = languageMessageTemplateGetResult.Data.Id;
            Active = languageMessageTemplateGetResult.Data.Active;
            MessageGroupId = languageMessageTemplateGetResult.Data.MessageGroupId;
            //MessageGroupName
            DeliveryProviderId = languageMessageTemplateGetResult.Data.DeliveryProviderId;
            //DeliveryProviderName
            Name = languageMessageTemplateGetResult.Data.Name;
            Default = languageMessageTemplateGetResult.Data.Default;
            Properties = messageTemplateProperties;
        }

        public LanguageMessageTemplatePutResult SubmitUpdate()
        {
            var properties = Properties.Select(item =>
                    new LanguageMessageTemplatePutInput_Property(
                        item.Id == -1 ? default(int?) : item.Id,
                        item.Active,
                        item.LanguageId,
                        item.Name,
                        !string.IsNullOrWhiteSpace(item.HtmlValue) ? item.HtmlValue : item.Value
                    )
                ).ToList();
            var languageMessageTemplatePutInput = new LanguageMessageTemplatePutInput(requestId: Guid.NewGuid().ToString(), TenantId, Id, Active, MessageGroupId, DeliveryProviderId, Name, Default, properties);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageTemplatePutResult languageMessageTemplatePutResult = Task.Run(() => languageWebClient.PutLanguageMessageTemplateAsync(languageMessageTemplatePutInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageMessageTemplatePutResult;
        }
    }
}
