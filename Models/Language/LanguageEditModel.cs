using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.Language;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.Language
{
    public class LanguageEditModel : LanguageModel
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        [Required]
        public new int Id { get; set; }

        public LanguageEditModel()
            : base()
        {
        }

        public LanguageEditModel(string tenantId, int id)
            : this()
        {
            var languageGetInput = new LanguageGetInput(requestId: Guid.NewGuid().ToString(), tenantId, id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageGetResult languageGetResult = Task.Run(() => languageWebClient.GetLanguageAsync(languageGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            if (languageGetResult == null || !languageGetResult.IsSuccessStatusCode || languageGetResult.Data == null)
                throw new Exception($"Failed to create language edit model: responseStatusCode={languageGetResult?.StatusCode} responseData={(languageGetResult?.Data == null ? "null" : "notnull")}");

            //
            Id = languageGetResult.Data.Id;
            Active = languageGetResult.Data.Active;
            Culture = languageGetResult.Data.Culture;
            Default = languageGetResult.Data.Default;
        }

        public LanguagePutResult SubmitUpdate(string tenantId)
        {
            var languagePutInput = new LanguagePutInput(requestId: Guid.NewGuid().ToString(), tenantId, Id, Active, Culture, Default);
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
