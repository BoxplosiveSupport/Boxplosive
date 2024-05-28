using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Management;
using nl.boxplosive.BackOffice.Mvc.Models.ModifyPromotion;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceContract.CampaignService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Utilities.Caching;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using CampaignEntityType = nl.boxplosive.Service.ServiceModel.Campaign.CampaignEntityType;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager, ReclamationEmployee, ReclamationManager")]
    public partial class ModifyPromotionController : PromotionControllerBase<ModifyPromotionModel>
    {
        // GET: ModifyCampaign
        public virtual ActionResult Index(int id)
        {
            ActionResult actionResult = GetActionResult(setReferrerAbsoluteUri: true, out bool redirectToHome);
            if (id < 1)
            {
                TempData["Warning"] = "The given Promotion couldn't be found!";

                return RedirectToAction(actionResult);
            }

            ModifyPromotionModel model;

            var request = new GetCampaignProcessRequest
            {
                Id = id
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Get");
            var response = ServiceCallFactory.CampaignProcess_Get(request);
            var campaign = (PromotionCampaign)response.Process.Campaign;
            var campaignProcess = response.Process;
            campaignProcess.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            model = PromotionFactory.ModifyCampaignModel(
                "Modify promotion",
                redirectToHome ? PageConst.Title_NavigateToHomePage : PageConst.Title_NavigateToManagementPublicationsPage,
                Url.Action(actionResult),
                string.Empty,
                campaign);

            model.Promotion.Basics.IsValid = TryValidateModel(model.Promotion.Basics);
            ModelState.Clear();
            model.Promotion.Segmentation.IsValid = TryValidateModel(model.Promotion.Segmentation);
            ModelState.Clear();
            model.Promotion.Timeframe.IsValid = TryValidateModel(model.Promotion.Timeframe);
            ModelState.Clear();
            model.Promotion.Discount.IsValid = TryValidateModel(model.Promotion.Discount);
            ModelState.Clear();
            model.Promotion.Promotion.IsValid = TryValidateModel(model.Promotion.Promotion);
            ModelState.Clear();

            model.StatusList.SetStatus(model.Promotion);

            Cache_Set_PromotionModel(model);

            return View(Views.ViewNames.Index, model);
        }

        public virtual ActionResult Modify(string id)
        {
            var returnId = 0;

            // Grab the correct campaign from the ID
            var request = new GetCampaignRequest
            {
                Id = id,
                Type = CampaignEntityType.Promotion
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Get");
            var responseGet = ServiceCallFactory.Campaign_Get(request);
            if (responseGet.Success)
            {
                PromotionCampaign campaignGet = responseGet.Campaign as PromotionCampaign;

                // Set the DateTimes as UTC
                campaignGet.StartDate = new DateTime(campaignGet.StartDate.Ticks, DateTimeKind.Utc);
                campaignGet.EndDate = new DateTime(campaignGet.EndDate.Ticks, DateTimeKind.Utc);
                campaignGet.VisibleFrom = new DateTime(campaignGet.VisibleFrom.Ticks, DateTimeKind.Utc);

                // Generate a new Modify draft
                var modifyRequest = new CampaignProcessRequest
                {
                    Process = new CampaignProcess
                    {
                        State = CampaignProcessState.Created,
                        Type = CampaignProcessType.Modify,
                        Campaign = campaignGet,
                    }
                };
                AuthenticationHelpers.SetupServiceRequest(modifyRequest, "Modify");
                var responseModify = ServiceCallFactory.CampaignProcess_Modify(modifyRequest);

                // Redirect to the index with the new draft id
                if (responseModify.Success)
                {
                    returnId = responseModify.Process.Id.Value;
                }
            }

            return RedirectToAction(MVC.ModifyPromotion.Index(returnId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Cancel()
        {
            // Delete the draft
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);
            if (process.State == CampaignProcessState.Created)
            {
                var request = new DeleteCampaignProcessRequest
                {
                    Process = process
                };
                AuthenticationHelpers.SetupServiceRequest(request, "Delete");
                ServiceCallFactory.CampaignProcess_Delete(request);
            }

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult ContinueLater()
        {
            Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState.Draft);

            TempData["Info"] = "The promotion has been saved as 'draft'.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult Publish()
        {
            if (PublishCachedPromotion())
                TempData["Success"] = "The promotion has been published.";
            else
                TempData["Danger"] = "The promotion hasn't been published.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        private bool PublishCachedPromotion()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);
            var campaign = process.Campaign as PromotionCampaign;
            process.State = CampaignProcessState.Publish;

            var requestContract = PromotionFactory.ConvertCampaignToPublishRequest(campaign, process);
            var publishresponse = ServiceCallFactory.CampaignProcess_Publish(requestContract);

            return publishresponse.Success;
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemovePromotion(int id)
        {
            ModifyPromotionModel model;
            var campaignProcess = CampaignFactory.GetCampaignProcess(id);
            var campaign = (PromotionCampaign)campaignProcess.Campaign;
            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            ActionResult actionResult = GetActionResult(setReferrerAbsoluteUri: false, out bool redirectToHome);
            model = PromotionFactory.ModifyCampaignModel(
                "Remove promotion",
                redirectToHome ? PageConst.Title_NavigateToHomePage : PageConst.Title_NavigateToManagementPublicationsPage,
                Url.Action(actionResult),
                string.Empty,
                campaign);
            model.Promotion.Id = campaignProcess.Id ?? 0;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemovePromotion(ModifyPromotionModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Promotion.Id) as CampaignProcess;
            process.State = CampaignProcessState.Cancelled;
            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Modify");
            var response = ServiceCallFactory.CampaignProcess_Modify(request);

            TempData["Success"] = "The promotion has been removed.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        public virtual ActionResult GetStepPartial(string id, string mode, int? ruleIndex)
        {
            var model = RetrieveModelFromCache(id);

            return ReturnCorrectView(id, mode, model, ruleIndex);
        }

        public virtual ActionResult GetStatusList()
        {
            var model = new StatusListModel();
            var campaignModel = Cache_Get_PromotionModel();
            model.SetStatus(campaignModel.Promotion);
            model.PromotionCode = campaignModel.PromotionCode;

            campaignModel.StatusList = model;
            Cache_Set_PromotionModel(campaignModel);

            return PartialView(MVC.ModifyPromotion.Views.DisplayTemplates.StatusListModel, model);
        }

        public virtual ActionResult SaveAsTemplate()
        {
            Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState.Template);

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        #region Step Setters

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepBasics(BasicStepModel model, string id, string mode)
        {
            var campaign = Cache_Get_Promotion();
            model.CopyValuesToServiceModel(campaign);
            Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);

            model = RetrieveModelFromCache(id) as BasicStepModel;
            var campaignModel = Cache_Get_PromotionModel();
            campaignModel.Promotion.Basics = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            Cache_Set_PromotionModel(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepTimeframe(PromotionTimeframeStepModel model, string id, string mode)
        {
            var campaign = Cache_Get_Promotion();

            var visibleFromDateTimeUtc = new DateTime(model.VisibleFromDate.Ticks + model.VisibleFromTime.Ticks, DateTimeKind.Local).ToUtcInOperatingTimeZone();
            var startDateTimeUtc = new DateTime(model.StartDate.Ticks + model.StartTime.Ticks, DateTimeKind.Local).ToUtcInOperatingTimeZone();
            var endDateTimeUtc = new DateTime(model.EndDate.Ticks + model.EndTime.Ticks, DateTimeKind.Local).ToUtcInOperatingTimeZone();
            var minStartDateTimeUtc = campaign.StartDate.ToUtcBeginOfDayInOperatingTimeZone() < OperatingDateTime.TodayUtc
                ? campaign.StartDate.ToUtcBeginOfDayInOperatingTimeZone()
                : OperatingDateTime.TodayUtc;
            if (visibleFromDateTimeUtc > startDateTimeUtc)
                ModelState.AddModelError("VisibleFromDate", "Visible from must be before or on Start");
            if (startDateTimeUtc < minStartDateTimeUtc)
                ModelState.AddModelError("StartDate", $"Start must be on or after {minStartDateTimeUtc.ToLocalTimeInOperatingTimeZone().ToString("g")}");
            if (endDateTimeUtc < startDateTimeUtc)
                ModelState.AddModelError("EndDate", "End must be after start");

            campaign.VisibleFrom = visibleFromDateTimeUtc;
            campaign.StartDate = startDateTimeUtc;
            campaign.EndDate = endDateTimeUtc;

            // FIX BPD-796 If Start/Enddates are equal set to 24h
            if (campaign.StartDate.Equals(campaign.EndDate))
                campaign.EndDate = campaign.EndDate.AddDays(1);

            Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);

            model = RetrieveModelFromCache(id) as PromotionTimeframeStepModel;
            var campaignModel = Cache_Get_PromotionModel();
            campaignModel.Promotion.Timeframe = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            Cache_Set_PromotionModel(campaignModel);

            return viewResult;
        }

        #endregion Step Setters

        #region Step Getters

        protected override StepModelBase RetrieveModelFromCache(string id)
        {
            var campaign = Cache_Get_Promotion();
            if (campaign == null)
                return null;

            switch (id.ToUpper())
            {
                case "BASICSTEP": // Step 1
                    {
                        return new BasicStepModel(campaign);
                    }
                case "SEGMENTATIONSTEP": // Step 2
                    {
                        var model = new SegmentationStepModel(campaign);

                        return model;
                    }
                case "TIMEFRAMESTEP": // Step 3
                    {
                        var model = new PromotionTimeframeStepModel
                        {
                            VisibleFromDate = campaign.VisibleFrom.ToLocalTimeInOperatingTimeZone().Date,
                            VisibleFromTime = campaign.VisibleFrom.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                            StartDate = campaign.StartDate.ToLocalTimeInOperatingTimeZone().Date,
                            StartTime = campaign.StartDate.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                            EndDate = campaign.EndDate.ToLocalTimeInOperatingTimeZone().Date,
                            EndTime = campaign.EndDate.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                        };

                        return model;
                    }
                case "DISCOUNTSTEP": // Step 4
                    {
                        var model = new DiscountStepModel
                        {
                            DiscountRules = campaign.Discounts == null
                                ? new List<DiscountRuleModel>()
                                : campaign.Discounts.Select((discount, index) => new DiscountRuleModel(discount, campaign, index)).ToList(),
                            IsPublished = true
                        };

                        return model;
                    }
                case "PROMOTIONSTEP": // Step 5
                    {
                        var model = new PromotionStepModel
                        {
                            ImageUrl = campaign.ImageFileName,
                            ImageEmail = campaign.ImageEmail
                        };

                        return model;
                    }
                default:
                    return null;
            }
        }

        protected override ActionResult ReturnCorrectView(string id, string mode, StepModelBase model, int? ruleIndex = null)
        {
            if (model != null)
            {
                model.IsValid = model.IsValid ?? ModelState.IsValid;
            }

            switch (id.ToUpper())
            {
                case "BASICSTEP": // Step 1
                    {
                        model = (BasicStepModel)model ?? new BasicStepModel();

                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.ModifyPromotion.Views.EditorTemplates.BasicStepModel
                            : MVC.ModifyPromotion.Views.DisplayTemplates.BasicStepModel, model);
                    }
                case "SEGMENTATIONSTEP": // Step 2
                    {
                        model = (SegmentationStepModel)model ?? new SegmentationStepModel();

                        return PartialView(mode.ToUpper() == "EDIT"
                            ? MVC.ModifyPromotion.Views.EditorTemplates.SegmentationStepModel
                            : MVC.ModifyPromotion.Views.DisplayTemplates.SegmentationStepModel, model);
                    }
                case "TIMEFRAMESTEP": // Step 3
                    {
                        model = (PromotionTimeframeStepModel)model ?? new PromotionTimeframeStepModel();

                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.ModifyPromotion.Views.EditorTemplates.TimeframeStepModel
                            : MVC.ModifyPromotion.Views.DisplayTemplates.TimeframeStepModel, model);
                    }
                case "DISCOUNTSTEP": // Step 4
                    {
                        model = (DiscountStepModel)model ?? new DiscountStepModel()
                        {
                            IsPublished = true
                        };
                        var discounts = model as DiscountStepModel;
                        if (ModelState.IsValid ||
                            (!ModelState.IsValid && ModelState.Keys.Count == 1
                            && ModelState.Keys.FirstOrDefault(k => k == "DiscountRules") != null))
                        {
                            if (!ruleIndex.HasValue && discounts.RuleType == 0)
                            {
                                return PartialView(mode.ToUpper() == "EDIT"
                                    ? MVC.Shared.Views.EditorTemplates.DiscountStepModel
                                    : MVC.ModifyPromotion.Views.DisplayTemplates.DiscountStepModel, model);
                            }
                        }

                        SetSelectedRule(discounts, ruleIndex);

                        return PartialView(MVC.ModifyPromotion.Views.EditorTemplates.DiscountRuleModel, discounts);
                    }
                case "PROMOTIONSTEP": // Step 5
                    {
                        model = (PromotionStepModel)model ?? new PromotionStepModel();

                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.ModifyPromotion.Views.EditorTemplates.PromotionStepModel
                            : MVC.ModifyPromotion.Views.DisplayTemplates.PromotionStepModel, model);
                    }
                default:
                    return null;
            }
        }

        protected override string ReturnDiscountRuleModelEditorTemplateName()
        {
            return MVC.ModifyPromotion.Views.EditorTemplates.DiscountRuleModel;
        }

        #endregion Step Getters

        #region Helpers

        private static readonly string _SessionKey_ReferrerAbsoluteUri = $"{nameof(ModifyPromotionController)}_ReferrerAbsoluteUri";

        /// <remarks>
        /// issue-14519: Fix that an action not always redirects to the home page.
        /// </remarks>
        private ActionResult GetActionResult(bool setReferrerAbsoluteUri, out bool redirectToHome)
        {
            string referrerAbsoluteUri;
            if (setReferrerAbsoluteUri)
            {
                referrerAbsoluteUri = Request.UrlReferrer.AbsoluteUri;
                Session[_SessionKey_ReferrerAbsoluteUri] = referrerAbsoluteUri;
            }
            else
                referrerAbsoluteUri = Session[_SessionKey_ReferrerAbsoluteUri] as string;

            if (!string.IsNullOrWhiteSpace(referrerAbsoluteUri) && referrerAbsoluteUri.IndexOf("/Management/Publications", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                redirectToHome = false;

                return MVC.Management.Publications(page: null, PublicationsType.Promotions);
            }

            redirectToHome = true;

            return MVC.Home.Index();
        }

        #endregion Helpers

        #region Cache

        private string _CacheKey_PromotionModel
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();

                return $"ModifyPromotionModel-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        private string _CacheKey_PromotionProcess
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();

                return $"ModifyPromotionProcess-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        protected override PromotionCampaign Cache_Get_Promotion()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);

            return process != null ? process.Campaign as PromotionCampaign : null;
        }

        protected override void Cache_Set_Promotion(PromotionCampaign campaign)
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);
            process.Campaign = campaign;

            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override void Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(PromotionCampaign campaign)
        {
            _Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaign);
        }

        protected override void Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState processState)
        {
            var campaignProcess = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);
            _Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(campaignProcess.Campaign, processState);
        }

        private void _Cache_Set_Promotion_AndInsertOrUpdateRepositoryRecord(CampaignBase campaignBase, CampaignProcessState? processState = null)
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_PromotionProcess);
            if (processState.HasValue)
                process.State = CampaignProcessState.Draft;

            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Modify");

            var response = ServiceCallFactory.CampaignProcess_Modify(request);
            process = response.Process;
            process.Campaign = campaignBase;

            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override ModifyPromotionModel Cache_Get_PromotionModel()
        {
            var model = DistributedCache.Instance.GetObject<ModifyPromotionModel>(_CacheKey_PromotionModel);

            return model;
        }

        protected override void Cache_Set_PromotionModel(ModifyPromotionModel model)
        {
            DistributedCache.Instance.SetObject(_CacheKey_PromotionModel, model, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        #endregion Cache
    }
}
