using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.Language;
using System;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.Language
{
    public class LanguageCreateModel : LanguageModel
    {
        public LanguageCreateModel()
            : base()
        {
        }

        public LanguagePutResult SubmitCreate(string tenantId)
        {
            var languagePutInput = new LanguagePutInput(requestId: Guid.NewGuid().ToString(), tenantId, id: null, Active, Culture, Default);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguagePutResult languagePutResult = Task.Run(() => languageWebClient.PutLanguageAsync(languagePutInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languagePutResult;
        }
    }
}
