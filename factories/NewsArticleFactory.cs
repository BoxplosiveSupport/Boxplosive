using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Models.ManageNewsArticle;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.BackOffice.Business.Data;
using nl.boxplosive.BackOffice.Business.Data.Entities;
using nl.boxplosive.Service.ServiceModel.Campaign;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class NewsArticleFactory : ViewModelBaseFactory<NewsArticleFactory>
    {
        public static ManageNewsArticleModel CreateManageNewsArticleModel(string pageTitle, string pageNavigationLabel, string pageNavigationUrl)
        {
            var model = new ManageNewsArticleModel();
            model.NewsArticles = NewsArticleRepository.GetAllNewsArticles()
                .Select(item => new NewsArticleModel(item))
                .OrderByDescending(item => item.StartDate)
                .ToList();

            // This sets the page values to the model
            return Instance.UpdateModel(pageTitle, pageNavigationLabel, pageNavigationUrl, model);
        }

        public static NewsArticleModel CreateNewsArticleModel(int newsArticleId)
        {
            NewsArticleModel model = null;
            if (newsArticleId > 0)
            {
                DtoNewsArticle dataModel = NewsArticleRepository.GetNewsArticle(newsArticleId);
                model = new NewsArticleModel(dataModel);

                model.Placeholders = GetPlaceholderModels(model.Id, model.AppCardTemplateId);
            }

            return model ?? new NewsArticleModel();
        }

        public static NewsArticleModel CreateNewsArticleModel(int newsArticleId, string pageTitle, string pageNavigationLabel, string pageNavigationUrl)
        {
            NewsArticleModel model = CreateNewsArticleModel(newsArticleId);

            // This sets the page values to the model
            return Instance.UpdateModel(pageTitle, pageNavigationLabel, pageNavigationUrl, model);
        }

        public static IList<PlaceholderModel> GetPlaceholderModels(int newsArticleId, int? appCardTemplateId)
        {
            IList<PlaceholderValue> servicePlaceholderValues =
                PlaceholderRepository.GetValuesByNewsArticle(newsArticleId).Select(item => new PlaceholderValue(item)).ToList();

            return PlaceholderFactory.GetPlaceholderModels(appCardTemplateId, servicePlaceholderValues);
        }

        #region Repositories

        protected static readonly INewsArticleRepository NewsArticleRepository = DataRepositoryFactory.GetInstance().DataRepository<INewsArticleRepository>();
        protected static readonly IPlaceholderRepository PlaceholderRepository = DataRepositoryFactory.GetInstance().DataRepository<IPlaceholderRepository>();

        #endregion Repositories
    }
}