using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using nl.boxplosive.Data.Sdk;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class EdmBatchOverviewModel : ViewModelBase
    {
        public EdmBatchOverviewModel()
        {
        }

        public EdmBatchOverviewModel(ICollection<IDtoEdmBatch> edmBatchDtos, int? pageNumber, string navigationUrl)
        {
            Batches = edmBatchDtos
                .Select(batch => new EdmBatchModel(batch))
                .OrderByDescending(batch => batch.CreatedAt)
                .ThenByDescending(batch => batch.Id)
                .ToPagedList(pageNumber ?? DefaultPageNumber, DefaultPageSize);

            PageTitle = "Management - Batches";
            PageNavigationUrl = navigationUrl;
            PageNavigationUrlText = PageConst.Title_NavigateToManagementPage;
        }

        public IPagedList<EdmBatchModel> Batches { get; }
    }
}