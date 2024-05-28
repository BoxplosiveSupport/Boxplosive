using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.Language;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.Language
{
    public class LanguageModel : VersionedViewModelBase
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
        public string Culture { get; set; }

        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public bool Default { get; set; }

        public LanguageModel()
            : base()
        {
            // Temp dummy value
            Version = Encoding.UTF8.GetBytes(1.ToString());
        }

        public LanguageDeleteResult SubmitDelete(string tenantId)
        {
            var languageDeleteInput = new LanguageDeleteInput(requestId: Guid.NewGuid().ToString(), tenantId, Id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeleteResult languageDeleteResult = Task.Run(() => languageWebClient.DeleteLanguageAsync(languageDeleteInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageDeleteResult;
        }
    }
}
