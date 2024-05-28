using nl.boxplosive.Data.Sdk.Enums;
using System;
using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public enum PublicationsType
    {
        Promotions = 0,
        Loyalties = 1,
        Rewards = 2,
        Savings = 3,
    }

    public static class PublicationsTypeExtensions
    {
        public static CampaignFilterType FromModel(this PublicationsType value)
        {
            if (value == PublicationsType.Loyalties)
                return CampaignFilterType.Loyalty;

            if (value == PublicationsType.Promotions)
                return CampaignFilterType.Promotion;

            if (value == PublicationsType.Rewards)
                return CampaignFilterType.Reward;

            if (value == PublicationsType.Savings)
                return CampaignFilterType.Saving;

            throw new NotImplementedException($"filter not supported type={value}");
        }

        public static PublicationsType ToModel(this CampaignFilterType value)
        {
            if (value == CampaignFilterType.Loyalty)
                return PublicationsType.Loyalties;

            if (value == CampaignFilterType.Promotion)
                return PublicationsType.Promotions;

            if (value == CampaignFilterType.Reward)
                return PublicationsType.Rewards;

            if (value == CampaignFilterType.Saving)
                return PublicationsType.Savings;

            throw new NotImplementedException($"filter not supported type={value}");
        }
    }

    public class PublicationsModel : ViewModelBase
    {
        public PublicationsModel()
        {
            Publications = new List<PublicationModel>();
        }

        public PublicationsType SelectedType { get; set; }

        public List<PublicationModel> Publications { get; set; }

        public PublicationFilterModel Filters { get; set; }

        public PaginationModel Pagination { get; set; }
    }
}