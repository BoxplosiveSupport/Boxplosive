using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageMessageGroup;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageMessageGroup
{
    public class LanguageMessageGroupModel : VersionedViewModelBase
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

        public LanguageMessageGroupModel()
            : base()
        {
            // Temp dummy value
            Version = Encoding.UTF8.GetBytes(1.ToString());
        }

        public LanguageMessageGroupDeleteResult SubmitDelete(string tenantId)
        {
            var languageMessageGroupDeleteInput = new LanguageMessageGroupDeleteInput(requestId: Guid.NewGuid().ToString(), tenantId, Id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageMessageGroupDeleteResult languageMessageGroupDeleteResult = Task.Run(() => languageWebClient.DeleteLanguageMessageGroupAsync(languageMessageGroupDeleteInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageMessageGroupDeleteResult;
        }

        public IList<SelectListItem> Names { get; } = new List<SelectListItem>()
        {
            new SelectListItem() { Text = "Email", Value = "Email" }
        };
    }
}
