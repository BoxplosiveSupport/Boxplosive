using nl.boxplosive.Data.Web;
using nl.boxplosive.Data.Web.Language.Models.LanguageDeliveryProvider;
using NLog;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageDeliveryProvider
{
    public class LanguageDeliveryProviderEditModel : LanguageDeliveryProviderModel
    {
        private static readonly Logger _Log = LogManager.GetCurrentClassLogger();

        [Required]
        public new int Id { get; set; }

        public LanguageDeliveryProviderEditModel()
            : base()
        {
        }

        public LanguageDeliveryProviderEditModel(string tenantId, int id)
            : this()
        {
            var languageDeliveryProviderGetInput = new LanguageDeliveryProviderGetInput(requestId: Guid.NewGuid().ToString(), tenantId, id);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeliveryProviderGetResult languageDeliveryProviderGetResult = Task.Run(() => languageWebClient.GetLanguageDeliveryProviderAsync(languageDeliveryProviderGetInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            if (languageDeliveryProviderGetResult == null || !languageDeliveryProviderGetResult.IsSuccessStatusCode || languageDeliveryProviderGetResult.Data == null)
                throw new Exception($"Failed to create language delivery provider edit model: responseStatusCode={languageDeliveryProviderGetResult?.StatusCode} responseData={(languageDeliveryProviderGetResult?.Data == null ? "null" : "notnull")}");

            //
            Id = languageDeliveryProviderGetResult.Data.Id;
            Active = languageDeliveryProviderGetResult.Data.Active;
            Name = languageDeliveryProviderGetResult.Data.Name;
            Properties = languageDeliveryProviderGetResult.Data.Properties.Select(item => new LanguageDeliveryProviderPropertyModel()
            {
                Id = item.Id,
                Active = item.Active,
                Name = item.Name,
                Value = item.Value,
            }).ToList();
        }

        public LanguageDeliveryProviderPutResult SubmitUpdate(string tenantId)
        {
            var properties = Properties.Select(item => new LanguageDeliveryProviderPutInput_Property(item.Id, item.Active, item.Name, item.Value)).ToList();
            var languageDeliveryProviderPutInput = new LanguageDeliveryProviderPutInput(requestId: Guid.NewGuid().ToString(), tenantId, Id, Active, Name, properties);
            // ELP-7088: Temporary workarround because ContextTask.RunWithResultAsync() crashes (no logging)
            var languageWebClient = WebClientManager.LanguageWebClient;
            LanguageDeliveryProviderPutResult languageDeliveryProviderPutResult = Task.Run(() => languageWebClient.PutLanguageDeliveryProviderAsync(languageDeliveryProviderPutInput)).GetAwaiter().GetResult();
            //BalanceGetResult webResult = ContextTask.RunWithResultAsync(async () =>
            //{
            //    return await balanceWebClient.GetBalanceAsync(webModel);
            //}).Result;

            return languageDeliveryProviderPutResult;
        }
    }
}