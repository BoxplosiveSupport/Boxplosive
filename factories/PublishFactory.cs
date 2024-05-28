using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models.Home;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class PublishFactory
    {
        public static PublishModel CreatePublishModel(CampaignType type, UrlHelper url)
        {
            var model = new PublishModel();

            // Collect only the given type, or collect all types
            var campaignTypes = Enum.GetValues(typeof(Service.ServiceModel.Campaign.CampaignEntityType)).Cast<Service.ServiceModel.Campaign.CampaignEntityType>();
            if (type != CampaignType.All)
            {
                campaignTypes = campaignTypes.Where(ct => ct.ToString() == type.ToString());
            }

            // Loop through the selected types
            foreach (var campaignType in campaignTypes)
            {
                var request = new GetAllCampaignProcessRequest
                {
                    CampaignType = campaignType
                };
                AuthenticationHelpers.SetupServiceRequest(request, "GetAll");
                var response = ServiceCallFactory.CampaignProcess_GetAll(request);
                if (response.Success)
                {
                    foreach (var process in response.Processes)
                    {
                        var draft = process.Campaign;

                        var module = new ProcessModule
                        {
                            Type = process.Tag ?? campaignType.ToString(),
                            ProcessType = process.Type,
                            Title = draft.Title ?? "Untitled",
                            StartDate = draft.StartDate.ToLocalTimeInOperatingTimeZone(),
                            EndDate = draft.EndDate.ToLocalTimeInOperatingTimeZone(),
                            ModifiedDate = process.ModifiedDate,
                            ActionUrl = GenerateActionUrl(url, process.Tag, campaignType, process),
                            DeleteUrl = GenerateDeleteUrl(url, process.Tag, campaignType, process),
                            ActionMetadata = GenerateActionMetadata(campaignType, process),
                            DeleteMetadata = GenerateDeleteMetadata(campaignType, process),
                        };

                        // Specific stuff
                        switch (campaignType)
                        {
                            case Service.ServiceModel.Campaign.CampaignEntityType.Promotion:
                                {
                                    var promotionDraft = process.Campaign as PromotionCampaign;
                                    module.IconImgUrl = promotionDraft.ImageFileName;
                                    break;
                                }
                            case Service.ServiceModel.Campaign.CampaignEntityType.Loyalty:
                                {
                                    var loyaltyDraft = process.Campaign as LoyaltyCampaign;
                                    module.IconImgUrl = loyaltyDraft.ImageFileName;
                                    break;
                                }
                        }

                        // Add it to the correct list
                        if (process.State == CampaignProcessState.Draft)
                        {
                            model.Drafts.Add(module);
                        }
                        else if (process.State == CampaignProcessState.Publish)
                        {
                            model.ReadyToPublish.Add(module);
                        }
                    }
                }
            }

            // Order the lists
            model.Drafts = model.Drafts.OrderByDescending(d => d.ModifiedDate).ToList();
            model.ReadyToPublish = model.ReadyToPublish.OrderByDescending(d => d.ModifiedDate).ToList();

            return model;
        }

        private static string GenerateActionUrl(UrlHelper url, string tag, Service.ServiceModel.Campaign.CampaignEntityType campaignType, CampaignProcess process)
        {
            switch (campaignType)
            {
                case Service.ServiceModel.Campaign.CampaignEntityType.Promotion:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return url.Action(MVC.ModifyPromotion.ActionNames.Index, MVC.ModifyPromotion.Name,
                                new { id = process.Id });

                        default:
                            switch (process.State)
                            {
                                case CampaignProcessState.Draft:
                                    return url.Action(MVC.CreatePromotion.ActionNames.ContinuePromotion,
                                        MVC.CreatePromotion.Name,
                                        new { id = process.Id });

                                case CampaignProcessState.Publish:
                                    return url.Action(MVC.PublishPromotion.ActionNames.ReadyToPublish,
                                        MVC.PublishPromotion.Name,
                                        new { id = process.Id });
                            }
                            break;
                    }
                    return string.Empty;

                case Service.ServiceModel.Campaign.CampaignEntityType.Loyalty:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return url.Action(MVC.ModifyLoyalty.ActionNames.Index, MVC.ModifyLoyalty.Name,
                                new { id = process.Id });

                        default:
                            switch (process.State)
                            {
                                case CampaignProcessState.Draft:
                                    return url.Action(MVC.CreateLoyalty.ActionNames.ContinueLoyalty,
                                        MVC.CreateLoyalty.Name,
                                        new { id = process.Id });

                                case CampaignProcessState.Publish:
                                    return url.Action(MVC.PublishLoyalty.ActionNames.ReadyToPublish,
                                        MVC.PublishLoyalty.Name,
                                        new { id = process.Id });
                            }
                            break;
                    }
                    return string.Empty;

                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Generates metadata corresponding to the ActionUrl created with GenerateActionUrl
        /// </summary>
        private static ActionMetadata GenerateActionMetadata(Service.ServiceModel.Campaign.CampaignEntityType campaignType, CampaignProcess process)
        {
            switch (campaignType)
            {
                case Service.ServiceModel.Campaign.CampaignEntityType.Promotion:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.ModifyPromotion.ActionNames.Index,
                                Namespace = MVC.ModifyPromotion.GetType().Namespace,
                                ControllerName = MVC.ModifyPromotion.Name,
                                IsClass = true
                            };

                        default:
                            switch (process.State)
                            {
                                case CampaignProcessState.Draft:
                                    return new ActionMetadata()
                                    {
                                        MethodName = MVC.CreatePromotion.ActionNames.ContinuePromotion,
                                        Namespace = MVC.CreatePromotion.GetType().Namespace,
                                        ControllerName = MVC.CreatePromotion.Name,
                                        IsClass = true
                                    };

                                case CampaignProcessState.Publish:
                                    return new ActionMetadata()
                                    {
                                        MethodName = MVC.PublishPromotion.ActionNames.ReadyToPublish,
                                        Namespace = MVC.PublishPromotion.GetType().Namespace,
                                        ControllerName = MVC.PublishPromotion.Name,
                                        IsClass = true,
                                    };
                            }
                            break;
                    }
                    return null;

                case Service.ServiceModel.Campaign.CampaignEntityType.Loyalty:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.ModifyLoyalty.ActionNames.Index,
                                Namespace = MVC.ModifyLoyalty.GetType().Namespace,
                                ControllerName = MVC.ModifyLoyalty.Name,
                                IsClass = true
                            };

                        default:
                            switch (process.State)
                            {
                                case CampaignProcessState.Draft:
                                    return new ActionMetadata()
                                    {
                                        MethodName = MVC.CreateLoyalty.ActionNames.ContinueLoyalty,
                                        Namespace = MVC.CreateLoyalty.GetType().Namespace,
                                        ControllerName = MVC.CreateLoyalty.Name,
                                        IsClass = true,
                                    };

                                case CampaignProcessState.Publish:
                                    return new ActionMetadata()
                                    {
                                        MethodName = MVC.PublishLoyalty.ActionNames.ReadyToPublish,
                                        Namespace = MVC.PublishLoyalty.GetType().Namespace,
                                        ControllerName = MVC.PublishLoyalty.Name,
                                        IsClass = true,
                                    };
                            }
                            break;
                    }
                    return null;

                default:
                    return null;
            }
        }

        private static string GenerateDeleteUrl(UrlHelper url, string tag, Service.ServiceModel.Campaign.CampaignEntityType campaignType, CampaignProcess process)
        {
            switch (campaignType)
            {
                case Service.ServiceModel.Campaign.CampaignEntityType.Promotion:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return url.Action(MVC.ModifyPromotion.ActionNames.RemovePromotion, MVC.ModifyPromotion.Name,
                                new { id = process.Id });

                        default:
                            return url.Action(MVC.CreatePromotion.ActionNames.RemovePromotion, MVC.CreatePromotion.Name,
                                new { id = process.Id });
                    }

                case Service.ServiceModel.Campaign.CampaignEntityType.Loyalty:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return url.Action(MVC.ModifyLoyalty.ActionNames.RemoveLoyalty, MVC.ModifyLoyalty.Name,
                                new { id = process.Id });

                        default:
                            return url.Action(MVC.CreateLoyalty.ActionNames.RemoveLoyalty, MVC.CreateLoyalty.Name,
                                new { id = process.Id });
                    }
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Generates metadata corresponding to the DeleteUrl created with GenerateDeleteUrl
        /// </summary>
        private static ActionMetadata GenerateDeleteMetadata(Service.ServiceModel.Campaign.CampaignEntityType campaignType, CampaignProcess process)
        {
            switch (campaignType)
            {
                case Service.ServiceModel.Campaign.CampaignEntityType.Promotion:

                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.ModifyPromotion.ActionNames.RemovePromotion,
                                Namespace = MVC.ModifyPromotion.GetType().Namespace,
                                ControllerName = MVC.ModifyPromotion.Name,
                                TypeArgs = new Type[] { typeof(int) },
                            };

                        default:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.CreatePromotion.ActionNames.RemovePromotion,
                                Namespace = MVC.CreatePromotion.GetType().Namespace,
                                ControllerName = MVC.CreatePromotion.Name,
                                TypeArgs = new Type[] { typeof(int) },
                            };
                    }
                case Service.ServiceModel.Campaign.CampaignEntityType.Loyalty:
                    switch (process.Type)
                    {
                        case CampaignProcessType.Modify:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.ModifyLoyalty.ActionNames.RemoveLoyalty,
                                Namespace = MVC.ModifyLoyalty.GetType().Namespace,
                                ControllerName = MVC.ModifyLoyalty.Name,
                                TypeArgs = new Type[] { typeof(int) },
                            };

                        default:
                            return new ActionMetadata()
                            {
                                MethodName = MVC.CreateLoyalty.ActionNames.RemoveLoyalty,
                                Namespace = MVC.CreateLoyalty.GetType().Namespace,
                                ControllerName = MVC.CreateLoyalty.Name,
                                TypeArgs = new Type[] { typeof(int) },
                            };
                    }
                default:
                    return null;
            }
        }
    }
}