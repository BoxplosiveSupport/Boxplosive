using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageTemplate;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageTemplate
{
    public class LanguageMessageTemplateCreateModel : LanguageMessageTemplateModel
    {
        public LanguageMessageTemplateCreateModel()
            : base()
        {
            SetMessageGroupNames();
            SetDeliveryProviderNames();
        }

        public LanguageMessageTemplatePutResult SubmitCreate()
        {
            var properties = Properties.Select(item =>
                    new LanguageMessageTemplatePutInput_Property(
                        id: null,
                        item.Active,
                        item.LanguageId,
                        item.Name,
                        !string.IsNullOrWhiteSpace(item.HtmlValue) ? item.HtmlValue : item.Value
                    )
                ).ToList();
            var languageMessageTemplatePutInput = new LanguageMessageTemplatePutInput(requestId: Guid.NewGuid().ToString(), TenantId, id: null, Active,
                    MessageGroupId, DeliveryProviderId, Name, Default, properties);
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
