using System.ComponentModel;
using System.Dynamic;
using nl.boxplosive.BackOffice.Mvc.Models;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class ViewModelBaseFactory<F> where F : class, new()
    {
        private static F _instance;

        public static F Instance
        {
            get { return _instance ?? (_instance = new F()); }
        }

        protected virtual T CreateModel<T>(string pageTitle, string pageNavigationLabel, string pageNavigationUrl) where T : ViewModelBase, new()
        {
            return new T
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrl,
                PageNavigationUrlText = pageNavigationLabel,
            };
        }

        protected virtual T UpdateModel<T>(string pageTitle, string pageNavigationLabel, string pageNavigationUrl, T model) where T : ViewModelBase, new()
        {
            model.PageTitle = pageTitle;
            model.PageNavigationUrl = pageNavigationUrl;
            model.PageNavigationUrlText = pageNavigationLabel;

            return model;
        }
    }
}