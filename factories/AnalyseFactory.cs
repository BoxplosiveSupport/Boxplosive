using System;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceContract.ReportingService.DataContracts;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class AnalyseFactory
    {
        public static AnalyseModel CreateAnalyseModel(UrlHelper url)
        {
            var model = new AnalyseModel();

            model.LiveCampaigns = CreateCampaignsModel(url, 1, 3, false, DateTime.UtcNow, DateTime.UtcNow);
            model.FinishedCampaigns = CreateCampaignsModel(url, 1, 3, true, DateTime.UtcNow, DateTime.UtcNow.AddDays(-30));

            model.AllLivePublicationUrl = url.Action(MVC.Home.ActionNames.ShowAllLiveCampaigns, MVC.Home.Name);
            model.AllFinishedPublicationUrl = url.Action(MVC.Home.ActionNames.ShowAllFinishedCampaigns, MVC.Home.Name);

            GetAllStatistics(model);

            return model;
        }

        public static CampaignsModel CreateCampaignsModel(UrlHelper url, int pageNumber, int pageSize, bool finishedCampaigns, DateTime startDate, DateTime endDate)
        {
            var request = new GetLiveOrFinishedCampaignsRequest
            {
                PageSize = pageSize,
                PageNumber = pageNumber,
                FinishedCampaigns = finishedCampaigns,
                StartDate = startDate,
                EndDate = endDate
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetLiveOrFinishedCampaigns");
            var response = ServiceCallFactory.ReportingService_GetLiveOrFinishedCampaigns(request);

            var model = new CampaignsModel();
            if (response.Success)
            {
                model.RowCount = response.RowCount;
                foreach (var campaignDetails in response.CampaignDetails)
                {
                    InternalCampaignType type = GetInternalCampaingType(campaignDetails.Type, campaignDetails.Id);

                    var campaign = new CampaignModel
                    {
                        Id = campaignDetails.Id,
                        Title = campaignDetails.Title,
                        IconImgUrl = campaignDetails.ImageUrl,
                        StartDate = campaignDetails.StartsAt.HasValue ? campaignDetails.StartsAt.Value.ToLocalTimeInOperatingTimeZone() :
                            (DateTime?)null,
                        ExpiryDate = campaignDetails.EndsAt.HasValue ? campaignDetails.EndsAt.Value.ToLocalTimeInOperatingTimeZone() :
                            (DateTime?)null,
                        Finished = finishedCampaigns,
                        ModifyUrl = GetActionUrl(url, type, campaignDetails.Id, isModify: true),
                        ModifyMetadata = GetModifyMetadata(type),
                        CopyUrl = GetActionUrl(url, type, campaignDetails.Id, isModify: false),
                        CopyMetadata = GetCopyMetadata(type),
                    };

                    model.Campaigns.Add(campaign);
                }
            }

            model.Pagination = new PaginationModel
            {
                PageNumber = pageNumber,
                ItemCount = model.RowCount,
                PageSize = pageSize
            };

            return model;
        }

        private static InternalCampaignType GetInternalCampaingType(string type, string id)
        {
            if (type == "Loyalty")
            {
                return InternalCampaignType.Loyalty;
            }

            return InternalCampaignType.Promotion;
        }

        /// <summary>
        /// Constructs the corresponding action url.
        /// </summary>
        /// <param name="url">The url helper to create an action url.</param>
        /// <param name="type">The internal campaign type.</param>
        /// <param name="id">The campaign id.</param>
        /// <param name="isModify">If the action is modify or else copy.</param>
        /// <returns>the constructed url.</returns>
        private static string GetActionUrl(UrlHelper url, InternalCampaignType type, string id, bool isModify)
        {
            if (type == InternalCampaignType.Loyalty)
            {
                return isModify
                    ? url.Action(MVC.ModifyLoyalty.ActionNames.Modify, MVC.ModifyLoyalty.Name, new { id })
                    : url.Action(MVC.CreateLoyalty.ActionNames.Copy, MVC.CreateLoyalty.Name, new { id });
            }

            return isModify
                ? url.Action(MVC.ModifyPromotion.ActionNames.Modify, MVC.ModifyPromotion.Name, new { id })
                : url.Action(MVC.CreatePromotion.ActionNames.Copy, MVC.CreatePromotion.Name, new { id });
        }

        /// <summary>
        /// Generates metadata corresponding to the ModifyUrl
        /// </summary>
        private static ActionMetadata GetModifyMetadata(InternalCampaignType type)
        {
            if (type == InternalCampaignType.Loyalty)
            {
                return new ActionMetadata()
                {
                    MethodName = MVC.ModifyLoyalty.ActionNames.Index,
                    Namespace = MVC.ModifyLoyalty.GetType().Namespace,
                    ControllerName = MVC.ModifyLoyalty.Name,
                    IsClass = true,
                };
            }

            return new ActionMetadata()
            {
                MethodName = MVC.ModifyPromotion.ActionNames.Index,
                Namespace = MVC.ModifyPromotion.GetType().Namespace,
                ControllerName = MVC.ModifyPromotion.Name,
                IsClass = true,
            };
        }

        /// <summary>
        /// Generates metadata corresponding to the CopyUrl
        /// </summary>
        private static ActionMetadata GetCopyMetadata(InternalCampaignType type)
        {
            if (type == InternalCampaignType.Loyalty)
            {
                return new ActionMetadata()
                {
                    MethodName = MVC.CreateLoyalty.ActionNames.Copy,
                    Namespace = MVC.CreateLoyalty.GetType().Namespace,
                    ControllerName = MVC.CreateLoyalty.Name,
                    TypeArgs = new Type[] { typeof(int) },
                };
            }

            return new ActionMetadata()
            {
                MethodName = MVC.CreatePromotion.ActionNames.Copy,
                Namespace = MVC.CreatePromotion.GetType().Namespace,
                ControllerName = MVC.CreatePromotion.Name,
                TypeArgs = new Type[] { typeof(string) },
            };
        }

        [Obsolete("ELP-7088 / ELP-8064: Enable statistics presented on the Analyse tab for Etos until they moved to the cloud")]
        private static void GetAllStatistics(AnalyseModel model)
        {
            if (AppConfig.Settings.Feature_ApiBalanceEnabled)
                return; // Do not retrieve statistics

            var request = new GetBasicStatisticsRequest
            {
            };

            AuthenticationHelpers.SetupServiceRequest(request, "GetBasicPlatformStatistics");

            var response = ServiceCallFactory.ReportingService_GetBasicStatistics(request);

            if (response.Success)
            {
                model.CashedCoupons = response.Statistics.CouponsCashed;
                model.ClaimedCoupons = response.Statistics.CouponsClaimed;
                model.IssuedCoupons = response.Statistics.CouponsIssued;

                model.GivenPoints = response.Statistics.PointsGiven;
                model.UsedPoints = response.Statistics.PointsUsed;

                model.TotalDiscountGiven = ((decimal)response.Statistics.TotalDiscount / 100);
            }
        }
    }
}