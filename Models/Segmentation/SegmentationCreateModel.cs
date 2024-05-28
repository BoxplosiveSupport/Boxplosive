using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Utilities.Json;
using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.Segmentation
{
    public class SegmentationCreateModel : SegmentationModel
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        public SegmentationCreateModel()
            : base()
        {
        }

        public DtoSegment SubmitCreate(string tenantId)
        {
            var segmentDto = new DtoSegment(id: 0, Name, createdOn: DateTime.UtcNow, updatedOn: null, deletedOn: null);
            DtoSegment newSegmentDto = SegmentationRepository.InsertOrUpdateSegment(segmentDto);
            if (newSegmentDto != null)
            {
                string value_s = string.Join(",", RetailerStoreIds);
                var segmentAttributeMappingDto = new DtoSegmentAttributeMapping(id: 0, newSegmentDto.Id, key: "favoriteStore", "IN", value_s, value_d: null);
                DtoSegmentAttributeMapping newSegmentAttributeMappingDto = SegmentationRepository.InsertOrUpdateSegmentAttributeMapping(segmentAttributeMappingDto);
                if (newSegmentAttributeMappingDto == null)
                {
                    if (_Logger.IsErrorEnabled)
                        _Logger.Error($"Failed to insert segment attribute mapping: dto={JsonWrapper.SerializeObject(segmentAttributeMappingDto)}");
                }
            }

            return newSegmentDto;
        }
    }
}
