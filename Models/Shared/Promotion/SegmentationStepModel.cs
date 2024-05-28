using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class SegmentationStepModel : StepModelBase
    {
        public readonly int SegmentId_Private = -1;
        public readonly int SegmentId_AllCustomers = -2;

        public int? DistributionId { get; set; }

        [DisplayName("Segment name")]
        [Required]
        public int SegmentId { get; set; }

        [DisplayName("Segment name")]
        public string SegmentName => SegmentNames.Single(item => item.Value.Equals(SegmentId.ToString(), StringComparison.OrdinalIgnoreCase)).Text;

        public IList<SelectListItem> SegmentNames { get; }

        public SegmentationStepModel()
        {
            IList<DtoSegment> segments = SegmentationRepository.GetSegments(mustBeActive: true);
            SegmentNames = new List<SelectListItem>()
            {
                new SelectListItem() { Text = "Private", Value = SegmentId_Private.ToString() },
                new SelectListItem() { Text = "All customers", Value = SegmentId_AllCustomers.ToString() },
            };
            SegmentNames.AddRange(segments.Select(item => new SelectListItem() { Text = item.Name, Value = item.Id.ToString() }).OrderBy(item => item.Text));
        }

        public SegmentationStepModel(PromotionCampaign serviceModel)
            : this()
        {
            bool isExistingEntity = !string.IsNullOrWhiteSpace(serviceModel.Id);

            DistributionId = serviceModel.DistributionId;
            if (serviceModel.SegmentId > 0 || serviceModel.SegmentId == SegmentId_Private || serviceModel.SegmentId == SegmentId_AllCustomers)
                SegmentId = serviceModel.SegmentId.Value;
            else if (isExistingEntity || serviceModel.IsCopy)
                SegmentId = serviceModel.NoDistribution ? SegmentId_Private : SegmentId_AllCustomers;
            else
                SegmentId = serviceModel.SegmentId ?? 0;
        }

        #region Repositories

        protected ISegmentationRepository SegmentationRepository = DataRepositoryFactory.GetInstance().DataRepository<ISegmentationRepository>();

        #endregion
    }
}