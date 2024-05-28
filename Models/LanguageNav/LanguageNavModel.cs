namespace nl.boxplosive.BackOffice.Mvc.Models.LanguageNav
{
    public class LanguageNavModel : ViewModelBase
    {
        public LanguageNavModel(string title, string navLabel, string navUrl)
        {
            PageTitle = title;
            PageNavigationUrl = navUrl;
            PageNavigationUrlText = navLabel;
        }
    }
}
