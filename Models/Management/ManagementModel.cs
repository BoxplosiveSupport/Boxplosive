namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class ManagementModel : ViewModelBase
    {
        public ManagementModel(string title, string navLabel, string navUrl)
        {
            PageTitle = title;
            PageNavigationUrl = navUrl;
            PageNavigationUrlText = navLabel;
        }
    }
}