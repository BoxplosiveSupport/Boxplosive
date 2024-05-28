using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Management;
using nl.boxplosive.Service.ServiceContract.LoyaltyPointService.DataContracts;
using nl.boxplosive.Service.ServiceContract.PublicationService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class PublicationsFactory
    {
        public static PublicationsModel CreatePublicationsModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrl,
            int pageNumber,
            int pageSize,
            PublicationsType type,
            UrlHelper url,
            PublicationFilterModel filters = null)
        {
            var model = new PublicationsModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrl,
                PageNavigationUrlText = pageNavigationLabel,
                SelectedType = type
            };

            model.Filters = filters ?? new PublicationFilterModel();

            switch (type)
            {
                case PublicationsType.Promotions:
                case PublicationsType.Rewards:
                case PublicationsType.Savings:
                    return FillModelWithPromotions(model, pageNumber, pageSize, url, type);

                case PublicationsType.Loyalties:
                    return FillModelWithLoyalties(model, pageNumber, pageSize, url);
            }

            return model;
        }

        private static PublicationsModel FillModelWithPromotions(PublicationsModel model, int pageNumber, int pageSize, UrlHelper url, PublicationsType type)
        {
            var request = new GetAllFilteredPublicationRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Title = !string.IsNullOrWhiteSpace(model.Filters.Title) ? model.Filters.Title : null,
                CampaignId = !string.IsNullOrWhiteSpace(model.Filters.CampaignId) ? model.Filters.CampaignId : null,
                VisibleFrom = model.Filters.VisibleFrom.HasValue ? model.Filters.VisibleFrom.Value.ToUtcInOperatingTimeZone() : (DateTime?)null,
                StartsAt = model.Filters.StartsAt().ToUtcInOperatingTimeZone(),
                EndsAt = model.Filters.EndsAt().ToUtcInOperatingTimeZone(),
                Status = model.Filters.Status,
                LoyaltyProgramId = 0,
                FilterType = type.FromModel(),
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetAll");

            var response = ServiceCallFactory.Publication_GetAllFiltered(request);
            if (response.Success)
            {
                // Filter duplicate publications (but different channel) for the same campaign
                // This is required because otherwise duplicates will be confusing for them
                var publications = new List<PublicationModel>();
                foreach (PromotionCampaign campaign in response.Campaigns)
                {
                    if (publications.All(p => p.Id != campaign.Id))
                    {
                        publications.Add(new PublicationModel
                        {
                            Id = campaign.Id,
                            Title = campaign.Title,
                            VisibleFrom = campaign.VisibleFrom,
                            StartDate = campaign.StartDate,
                            ExpiryDate = campaign.EndDate,
                            Status = campaign.Status.ToString(),
                            ModifyUrl = (type == PublicationsType.Promotions ?
                                url.Action(MVC.ModifyPromotion.ActionNames.Modify, MVC.ModifyPromotion.Name,
                                    new { id = campaign.Id })
                                : null),
                            CopyUrl = (type == PublicationsType.Promotions ?
                                url.Action(MVC.CreatePromotion.ActionNames.Copy, MVC.CreatePromotion.Name,
                                    new { id = campaign.Id })
                                : null),
                            ConfigurationUrl = (type == PublicationsType.Loyalties ?
                                url.Action(MVC.LoyaltyProgram.ActionNames.Live, MVC.LoyaltyProgram.Name,
                                    new { loyaltyProgram = campaign.Id })
                                : null)
                        });
                    }
                }

                model.Publications = publications;

                // Collect page count
                // Copy filters from the normal request
                var countRequest = new GetCountPublicationRequest
                {
                    Title = model.Filters.Title,
                    CampaignId = model.Filters.CampaignId,
                    VisibleFrom = model.Filters.VisibleFrom.HasValue ? model.Filters.VisibleFrom.Value.ToUtcInOperatingTimeZone() : (DateTime?)null,
                    StartsAt = model.Filters.StartsAt().ToUtcInOperatingTimeZone(),
                    EndsAt = model.Filters.EndsAt().ToUtcInOperatingTimeZone(),
                    Status = model.Filters.Status,
                    LoyaltyProgramId = 0,
                    FilterType = type.FromModel(),
                };
                AuthenticationHelpers.SetupServiceRequest(countRequest, "GetCount");
                var countResponse = ServiceCallFactory.Publication_GetCount(countRequest);

                model.Pagination = new PaginationModel
                {
                    PageNumber = pageNumber,
                    ItemCount = countResponse.Success ? countResponse.Count : 0,
                    PageSize = pageSize
                };
            }
            else
            {
                // TODO Show / log
            }

            return model;
        }

        private static PublicationsModel FillModelWithLoyalties(PublicationsModel model, int pageNumber, int pageSize, UrlHelper url)
        {
            int loyaltyProgramId;
            if (model.Filters.CampaignId != null && !int.TryParse(model.Filters.CampaignId, out loyaltyProgramId))
            {
                model.Pagination = new PaginationModel
                {
                    PageNumber = pageNumber,
                    ItemCount = 0,
                    PageSize = pageSize
                };

                return model;
            }

            var request = new GetFilteredLoyaltyPointsProgramsRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Title = !string.IsNullOrWhiteSpace(model.Filters.Title) ? model.Filters.Title : null,
                // CampaignId == LoyaltyPointsProgramId, so only set value when it is an int, otherwise -1 so that no results are found for this filter value
                CampaignId = !string.IsNullOrWhiteSpace(model.Filters.CampaignId)
                    ? int.TryParse(model.Filters.CampaignId, out int filterLoyaltyProgramId) ? filterLoyaltyProgramId.ToString() : "-1"
                    : null,
                StartsAt = model.Filters.StartsAt().ToUtcInOperatingTimeZone(),
                EndsAt = model.Filters.EndsAt().ToUtcInOperatingTimeZone(),
                Status = model.Filters.Status
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetAll");

            var response = ServiceCallFactory.LoyaltyPoints_GetFilteredPrograms(request);
            if (response.Success)
            {
                foreach (var loyalty in response.LoyaltyPointsPrograms)
                {
                    var pubModel = new PublicationModel
                    {
                        Id = loyalty.Id.ToString(),
                        Title = loyalty.Name,
                        StartDate = loyalty.StartsAt,
                        ExpiryDate = loyalty.EndsAt,
                        Status = loyalty.Status.ToString(),
                        IsPublic = loyalty.IsPublic,
                        ModifyUrl = url.Action(MVC.ModifyLoyalty.ActionNames.Modify, MVC.ModifyLoyalty.Name, new { id = loyalty.Id }),
                        CopyUrl = url.Action(MVC.CreateLoyalty.ActionNames.Copy, MVC.CreateLoyalty.Name, new { id = loyalty.Id }),
                        ReminderUrl = url.Action(MVC.LoyaltyRewardReminder.ActionNames.Index, MVC.LoyaltyRewardReminder.Name, new { id = loyalty.Id }),
                        ConfigurationUrl = url.Action(MVC.LoyaltyProgram.ActionNames.Live, MVC.LoyaltyProgram.Name, new { loyaltyProgram = loyalty.Id })
                    };

                    model.Publications.Add(pubModel);
                }

                // Collect page count
                // Copy filters from the normal request
                var countRequest = new GetFilteredLoyaltyPointsProgramsCountRequest
                {
                    Title = request.Title,
                    CampaignId = request.CampaignId,
                    StartsAt = request.StartsAt,
                    EndsAt = request.EndsAt,
                    Status = request.Status,
                };
                AuthenticationHelpers.SetupServiceRequest(countRequest, "GetCount");
                var countResponse = ServiceCallFactory.LoyaltyPoints_GetFilteredProgramsCount(countRequest);

                model.Pagination = new PaginationModel
                {
                    PageNumber = pageNumber,
                    ItemCount = countResponse.Success ? countResponse.Count : 0,
                    PageSize = pageSize
                };
            }
            else
            {
                // TODO Show / log
            }

            return model;
        }
    }
}