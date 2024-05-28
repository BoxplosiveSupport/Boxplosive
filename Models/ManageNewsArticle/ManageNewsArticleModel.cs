using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageNewsArticle
{
	public class ManageNewsArticleModel : ViewModelBase
	{
		public List<NewsArticleModel> NewsArticles { get; set; }
	}
}