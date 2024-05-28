using Foolproof;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Utilities.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Models.Segmentation
{
    public class SegmentationEditModel : SegmentationModel
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        [Required]
        public new int Id { get; set; }

        public bool IsDeleted { get; set; }

        [RequiredIfTrue(nameof(IsDeleted), ErrorMessage = "The Undelete field is required.")]
        public bool Undelete { get; set; }

        public SegmentationEditModel()
            : base()
        {
        }

        public SegmentationEditModel(string tenantId, int id)
            : this()
        {
            DtoSegment segmentDto = SegmentationRepository.GetSegment(id);
            if (segmentDto == null)
                throw new Exception($"Failed to create segment edit model: id={id}");

            IList<DtoSegmentAttributeMapping> segmentAttributeMappingDtos = SegmentationRepository.GetSegmentAttributeMappingsBySegmentId(segmentDto.Id);

            Id = segmentDto.Id;
            Name = segmentDto.Name;
            IsDeleted = segmentDto.DeletedOn.HasValue;
            RetailerStoreIds = new HashSet<int>();

            foreach (DtoSegmentAttributeMapping segmentAttributeMappingDto in segmentAttributeMappingDtos)
            {
                if (segmentAttributeMappingDto.Key.Equals("favoriteStore", StringComparison.OrdinalIgnoreCase))
                {
                    foreach (string value in (segmentAttributeMappingDto.Value_s ?? string.Empty).Split(',').Where(item => !string.IsNullOrWhiteSpace(item)).ToList())
                    {
                        if (int.TryParse(value, out int intValue))
                            RetailerStoreIds.Add(intValue);
                    }
                }
                else
                {
                    if (_Logger.IsWarnEnabled)
                        _Logger.Warn($"Segment attribute mapping is not supported: id={segmentAttributeMappingDto.Id} segmentId={segmentAttributeMappingDto.SegmentId} key={segmentAttributeMappingDto.Key}");
                }
            }
        }

        public DtoSegment SubmitUpdate(string tenantId)
        {
            DtoSegment segmentDto = SegmentationRepository.GetSegment(Id);
            if (segmentDto == null)
                throw new Exception($"Failed to update segment: id={Id} name={Name}");

            segmentDto.Name = Name;
            segmentDto.UpdatedOn = DateTime.UtcNow;
            segmentDto.DeletedOn = null;

            DtoSegment newSegmentDto = SegmentationRepository.InsertOrUpdateSegment(segmentDto);
            if (newSegmentDto != null)
            {
                IList<DtoSegmentAttributeMapping> segmentAttributeMappingDtos = SegmentationRepository.GetSegmentAttributeMappingsBySegmentId(newSegmentDto.Id);
                DtoSegmentAttributeMapping segmentAttributeMappingDto = segmentAttributeMappingDtos.FirstOrDefault(item => item.Key.Equals("favoriteStore", StringComparison.OrdinalIgnoreCase));

                string value_s = string.Join(",", RetailerStoreIds);
                if (segmentAttributeMappingDto == null)
                {
                    segmentAttributeMappingDto = new DtoSegmentAttributeMapping(id: 0, newSegmentDto.Id, key: "favoriteStore", "IN", value_s, value_d: null);
                }
                else
                {
                    segmentAttributeMappingDto.Value_s = value_s;
                    segmentAttributeMappingDto.Value_d = null;
                }

                DtoSegmentAttributeMapping newSegmentAttributeMappingDto = SegmentationRepository.InsertOrUpdateSegmentAttributeMapping(segmentAttributeMappingDto);
                if (newSegmentAttributeMappingDto == null)
                {
                    if (_Logger.IsErrorEnabled)
                        _Logger.Error($"Failed to update segment attribute mapping: dto={JsonWrapper.SerializeObject(segmentAttributeMappingDto)}");
                }
            }

            return newSegmentDto;
        }
    }
}
