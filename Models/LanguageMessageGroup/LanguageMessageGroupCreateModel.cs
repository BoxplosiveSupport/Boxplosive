using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using System;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageGroup
{
    public class LanguageMessageGroupCreateModel : LanguageMessageGroupModel
    {
        public LanguageMessageGroupCreateModel()
            : base()
        {
        }

        public LanguageMessageGroupPutResult SubmitCreate(string tenantId)
        {
            var languageMessageGroupPutInput = new LanguageMessageGroupPutInput(requestId: Guid.NewGuid().ToString(), tenantId, id: null, Active, Name);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageGroupPutResult languageMessageGroupPutResult = Task.Run(() => languageWebClient.PutLanguageMessageGroupAsync(languageMessageGroupPutInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageMessageGroupPutResult;
        }
    }
}
