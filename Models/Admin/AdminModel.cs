using nl.boxplosive.Configuration;

namespace nl.boxplosive.BackOffice.Mvc.Models.Admin
{
    public class AdminModel : ViewModelBase
    {
        public bool IsWebsealAuthenticationEnabled => AppConfig.Settings.WebSealEnabled;
    }
}