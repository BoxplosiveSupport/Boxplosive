using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Management;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Utilities.Caching;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class ManagementController : ControllerBase
    {
        private string _CacheKey_PublicationsFilterModel
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();
                return $"PublicationsFilterModel-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        private string _CacheKey_PublicationsType
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();
                return $"PublicationsType-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        private const int PageSize = 10;

        protected string ManagementIndexNavigationUrl => Url.Action(ActionNameConstants.Index, NameConst);

        protected string ManagementEdmBatchesNavigationUrl => Url.Action(ActionNameConstants.EdmBatches, NameConst);

        // GET: Management
        public virtual ActionResult Index()
        {
            return View(new ManagementModel(
                "Management",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst)
            ));
        }

        #region Publications

        public delegate ActionResult PublicationsDelegate(int? page, PublicationsType? type);

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, ReclamationEmployee, ReclamationManager")]
        public virtual ActionResult Publications(int? page, PublicationsType? type)
        {
            var pageNumber = 1;
            if (page.HasValue && page.Value > 1)
                pageNumber = page.Value;

            var filterModel = DistributedCache.Instance.GetObject<PublicationFilterModel>(_CacheKey_PublicationsFilterModel);

            if (type == null)
            {
                var cacheType = DistributedCache.Instance.GetObject<string>(_CacheKey_PublicationsType);
                type = string.IsNullOrEmpty(cacheType)
                    ? PublicationsType.Promotions
                    : EnumHelper<PublicationsType>.Parse(cacheType);
            }

            DistributedCache.Instance.SetObject(_CacheKey_PublicationsType, type.ToString(), CacheSettings.DefaultRelativeExpiration_Backoffice);

            var model = PublicationsFactory.CreatePublicationsModel("Management - Publications",
                PageConst.Title_NavigateToManagementPage,
                Url.Action(ActionNameConstants.Index, NameConst),
                pageNumber,
                PageSize,
                type.Value,
                Url,
                filterModel);

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Publications(PublicationsModel model)
        {
            if (model.Filters.HasFilters)
            {
                // Cache the filters
                DistributedCache.Instance.SetObject(_CacheKey_PublicationsFilterModel, model.Filters, CacheSettings.DefaultRelativeExpiration_Backoffice);
            }
            else
            {
                // Clear the cache
                DistributedCache.Instance.Remove(_CacheKey_PublicationsFilterModel);
            }

            // Redirect to the normal view to reset everything
            return RedirectToAction("Publications");
        }

        #endregion Publications

        #region Background processes

        public delegate ActionResult BackgroundProcessesDelegate();

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult BackgroundProcesses()
        {
            var model = CronJobFactory.CreateBackgroundProcessesModel(
                "Background processes",
                PageConst.Title_NavigateToManagementPage,
                Url.Action(ActionNameConstants.Index, NameConst),
                Url);

            return View(model);
        }

        #endregion Background processes

        #region EDM Batches

        public delegate System.Web.Mvc.ActionResult EdmBatchesDelegate(int? page);

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult EdmBatches(int? page)
        {
            var model = new EdmBatchOverviewModel(EdmBatchRepository.GetAll(), page, ManagementIndexNavigationUrl);

            return View(model);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult EdmBatch(int id)
        {
            var model = new EdmBatchDetailsModel(EdmBatchRepository.Get(id), EdmBatchRepository.GetBatchDetails(id), ManagementEdmBatchesNavigationUrl);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult EdmBatch(EdmBatchDetailsModel model)
        {
            List<EdmBatchCheckPromotionModel> selectedPromotions = model.Promotions.Where(p => p.IsChecked).ToList();
            if (selectedPromotions.Count > 0)
            {
                var subjectAndArguments = new EdmBatchCancelReservationsEventSubjectAndArguments();
                subjectAndArguments.SetSubject(new EdmBatchCancelReservationsEventSubject(model.Batch.Id));
                subjectAndArguments.SetArguments(selectedPromotions.Select(p => new EdmBatchCancelReservationsEventArgument(p.Id)).ToList());

                CampaignApi.AddEdmBatchCancelReservationsEvent(subjectAndArguments);

                TempData["Success"] = $"Cancel reservations event for batch {model.Batch.Id} has been scheduled.";
            }
            else
            {
                TempData["Warning"] = $"No promotions selected. Cancel reservations event for batch {model.Batch.Id} has not been scheduled.";
            }

            return RedirectToAction(ActionNameConstants.EdmBatches);
        }

        #endregion EDM Batches
    }
}