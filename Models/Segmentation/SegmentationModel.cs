using nl.boxplosive.Data.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Segmentation
{
    public class SegmentationModel : VersionedViewModelBase
    {
        /// <remarks>
        /// Required.
        /// </remarks>
        [Required]
        public string Name { get; set; }

        public IList<SelectListItem> RetailerStores
        {
            get
            {
                if (_RetailerStores == null)
                {
                    IList<DtoRetailerStoreBasic> retailerStoreDtos = RetailerRepository.GetAllStores();

                    _RetailerStores = new List<SelectListItem>();
                    foreach (DtoRetailerStoreBasic retailerStoreDto in retailerStoreDtos.OrderBy(item => item.Name).ToList())
                    {
                        _RetailerStores.Add(new SelectListItem() { Text = $"{retailerStoreDto.Name} ({retailerStoreDto.Number})", Value = retailerStoreDto.Id.ToString() });
                    }
                }

                return _RetailerStores;
            }
        }
        private IList<SelectListItem> _RetailerStores { get; set; }

        [DisplayName("Retailer stores")]
        [Required]
        public HashSet<int> RetailerStoreIds { get; set; }

        public SegmentationModel()
            : base()
        {
            // Temp dummy value
            Version = Encoding.UTF8.GetBytes(1.ToString());
        }

        public (bool yes, IList<DtoDistribution> distributions) CanDelete(string tenantId)
        {
            IList<DtoDistribution> distributions = SegmentationRepository.GetDistributionsBySegmentId(Id, mustBeActive: false, DistributionEntityTypes.None);

            return (yes: !distributions.Any(), distributions);
        }

        public DtoSegment SubmitDelete(string tenantId)
        {
            DtoSegment segmentDto = SegmentationRepository.GetSegment(Id);
            if (segmentDto == null)
                throw new Exception($"Failed to delete segment: id={Id} name={Name}");

            segmentDto.Name = Name;
            segmentDto.UpdatedOn = DateTime.UtcNow;
            segmentDto.DeletedOn = DateTime.UtcNow;

            return SegmentationRepository.InsertOrUpdateSegment(segmentDto);
        }
    }
}
