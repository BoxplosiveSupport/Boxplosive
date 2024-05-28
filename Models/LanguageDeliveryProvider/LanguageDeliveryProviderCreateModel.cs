using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageDeliveryProvider
{
    public class LanguageDeliveryProviderCreateModel : LanguageDeliveryProviderModel
    {
        public LanguageDeliveryProviderCreateModel()
            : base()
        {
        }

        public LanguageDeliveryProviderPutResult SubmitCreate(string tenantId)
        {
            var properties = Properties.Select(item => new LanguageDeliveryProviderPutInput_Property(id: null, item.Active, item.Name, item.Value)).ToList();
            var languageDeliveryProviderPutInput = new LanguageDeliveryProviderPutInput(requestId: Guid.NewGuid().ToString(), tenantId, id: null, Active, Name, properties);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeliveryProviderPutResult languagePutResult = Task.Run(() => languageWebClient.PutLanguageDeliveryProviderAsync(languageDeliveryProviderPutInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languagePutResult;
        }
    }
}