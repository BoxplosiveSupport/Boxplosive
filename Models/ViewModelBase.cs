using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public abstract class ViewModelBase
    {
        protected static readonly int DefaultPageNumber = 1;
        protected static readonly int DefaultPageSize = 10;

        protected const string DateFormatString = "{0:dd-MM-yyyy}";
        protected const string TimeFormatString = "{0:HH\\:mm}";
        protected const string DateTimeFormatString = "{0:dd-MM-yyyy HH\\:mm}";

        protected const string TimeSpanRegEx = @"^((^$)|(^[0-9]{1,7}$)|(([0-9]{1,7})\.([0-9]|0[0-9]|1[0-9]|2[0-3]):([0-9]|[0-5][0-9])))$";
        protected const string TimeSpanRegExMessage = "The value must be in days or in a timespan format e.g. d.hh:mm.";

        public string PageTitle { get; set; }
        public string PageNavigationTitle { get; set; }
        public string PageNavigationUrl { get; set; }
        public string PageNavigationUrlText { get; set; }

        public int Page { get; set; }

        public void SetPageFields(int? page, string pageTitle, string pageNavigationUrl, string pageNavigationUrlText)
        {
            string pageNavigationTitle = null;
            SetPageFields(page, pageTitle, pageNavigationTitle, pageNavigationUrl, pageNavigationUrlText);
        }

        protected void SetPageFields(int? page, string pageTitle, string pageNavigationTitle, string pageNavigationUrl, string pageNavigationUrlText)
        {
            Page = page ?? PaginationModel.DefaultPageNumber;
            PageTitle = pageTitle ?? pageNavigationTitle;
            PageNavigationTitle = pageNavigationTitle ?? pageTitle;
            PageNavigationUrl = pageNavigationUrl;
            PageNavigationUrlText = pageNavigationUrlText;
        }

        public static Modal GetConfirmDeleteModal(string itemName)
        {
            return new Modal("confirm-delete-modal")
            {
                Title = $"Delete {itemName.ToLower()}",
                Description = $"<strong>You are about to delete {itemName}:</strong>",
                Question = "<br /><p>Do you want to continue?</p>"
            };
        }

        public static ModalInfo GetInfoModal(string id, string title, string description)
        {
            return new ModalInfo($"info-modal-{id}")
            {
                Title = title,
                Description = description,
            };
        }

        #region Repositories

        protected IAccountRepository AccountRepository = DataRepositoryFactory.GetInstance().DataRepository<IAccountRepository>();
        protected IAppConfigItemRepository AppConfigItemRepository = DataRepositoryFactory.GetInstance().DataRepository<IAppConfigItemRepository>();
        protected ICustomerProfileRepository CustomerProfileRepository => DataRepositoryFactory.GetInstance().DataRepository<ICustomerProfileRepository>();
        protected IIssuerRepository IssuerRepository => DataRepositoryFactory.GetInstance().DataRepository<IIssuerRepository>();
        protected IPlaceholderRepository PlaceholderRepository => DataRepositoryFactory.GetInstance().DataRepository<IPlaceholderRepository>();
        protected IRetailerRepository RetailerRepository => DataRepositoryFactory.GetInstance().DataRepository<IRetailerRepository>();
        protected ISegmentationRepository SegmentationRepository => DataRepositoryFactory.GetInstance().DataRepository<ISegmentationRepository>();
        protected ISessionRepository SessionRepository => DataRepositoryFactory.GetInstance().DataRepository<ISessionRepository>();
        protected ITemplateRepository TemplateRepository => DataRepositoryFactory.GetInstance().DataRepository<ITemplateRepository>();

        #endregion Repositories
    }
}