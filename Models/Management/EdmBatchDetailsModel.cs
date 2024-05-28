using nl.boxplosive.Data.Sdk;
using System.Collections.Generic;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class EdmBatchDetailsModel : ViewModelBase
    {
        public EdmBatchDetailsModel()
        {
            Batch = new EdmBatchModel();
            Promotions = new List<EdmBatchCheckPromotionModel>();
        }

        public EdmBatchDetailsModel(IDtoEdmBatch edmBatchDto, ICollection<DtoEdmBatchPromotion> edmBatchPromotionDtos, string navigationUrl)
        {
            Batch = new EdmBatchModel(edmBatchDto);
            Promotions = edmBatchPromotionDtos
                .Select(promo => new EdmBatchCheckPromotionModel(promo))
                .OrderBy(promo => promo.Id)
                .ToList();

            PageTitle = $"Management - Cancel batch {Batch.Id}";
            PageNavigationUrl = navigationUrl;
            PageNavigationUrlText = $"{PageConst.Title_NavigateToManagementPage} - Batches";
        }

        public EdmBatchModel Batch { get; }

        public IList<EdmBatchCheckPromotionModel> Promotions { get; }
    }
}