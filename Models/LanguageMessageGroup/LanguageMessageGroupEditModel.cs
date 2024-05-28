using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageGroup
{
    public class LanguageMessageGroupEditModel : LanguageMessageGroupModel
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        [Required]
        public new int Id { get; set; }

        public LanguageMessageGroupEditModel()
            : base()
        {
        }

        public LanguageMessageGroupEditModel(string tenantId, int id)
            : this()
        {
            var languageMessageGroupGetInput = new LanguageMessageGroupGetInput(requestId: Guid.NewGuid().ToString(), tenantId, id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageGroupGetResult languageMessageGroupGetResult = Task.Run(() => languageWebClient.GetLanguageMessageGroupAsync(languageMessageGroupGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            if (languageMessageGroupGetResult == null || !languageMessageGroupGetResult.IsSuccessStatusCode || languageMessageGroupGetResult.Data == null)
                throw new Exception($"Failed to create language message group edit model: responseStatusCode={languageMessageGroupGetResult?.StatusCode} responseData={(languageMessageGroupGetResult?.Data == null ? "null" : "notnull")}");

            //
            Id = languageMessageGroupGetResult.Data.Id;
            Active = languageMessageGroupGetResult.Data.Active;
            Name = languageMessageGroupGetResult.Data.Name;
        }

        public LanguageMessageGroupPutResult SubmitUpdate(string tenantId)
        {
            var languageMessageGroupPutInput = new LanguageMessageGroupPutInput(requestId: Guid.NewGuid().ToString(), tenantId, Id, Active, Name);
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
