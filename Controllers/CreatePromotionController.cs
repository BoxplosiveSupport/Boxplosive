using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.CreatePromotion;
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
using WebGrease.Css.Extensions;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager, ReclamationEmployee")]
    public partial class CreatePromotionController : PromotionControllerBase<CreatePromotionModel>
    {
        // GET: CreateCampaign
        public virtual ActionResult Index()
        {
            DateTime startDate = OperatingDateTime.TodayUtc;
            var promotionCampaign = new PromotionCampaign
            {
                StartDate = startDate,
                EndDate = OperatingDateTime.EndOfDayUtc(TimeSpan.FromDays(7)).FloorByMinute(),
                VisibleFrom = startDate,
            };

            var request = new CampaignProcessRequest
            {
                Process = new CampaignProcess
                {
                    State = CampaignProcessState.Created,
                    Type = CampaignProcessType.Create
                }
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            var campaignProcess = response.Process;
            response.Process.Campaign = promotionCampaign;
            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            var model = PromotionFactory.CreateCampaignModel(
                "Create promotion",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                campaign: null);

            Cache_Set_PromotionModel(model);
            return View(model);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult Copy(string id)
        {
            // Grab the correct campaign from the ID
            var requestcopy = new GetCampaignRequest
            {
                Id = id,
                Type = Service.ServiceModel.Campaign.CampaignEntityType.Promotion
            };
            AuthenticationHelpers.SetupServiceRequest(requestcopy, "Get");
            var responseGet = ServiceCallFactory.Campaign_Get(requestcopy);
            if (responseGet.Success)
            {
                PromotionCampaign campaignGet = responseGet.Campaign as PromotionCampaign;
                campaignGet.IsCopy = true;

                // Set the DateTimes as UTC
                campaignGet.StartDate = new DateTime(campaignGet.StartDate.Ticks, DateTimeKind.Utc);
                campaignGet.EndDate = new DateTime(campaignGet.EndDate.Ticks, DateTimeKind.Utc);
                campaignGet.VisibleFrom = new DateTime(campaignGet.VisibleFrom.Ticks, DateTimeKind.Utc);

                // Clear campaign
                campaignGet.Id = string.Empty;
                campaignGet.PlaceholderValues.ForEach(item => item.Id = 0);
                campaignGet.Discounts.ForEach(d => d.Id = 0);

                var request = new CampaignProcessRequest
                {
                    Process = new CampaignProcess
                    {
                        State = CampaignProcessState.Created,
                        Type = CampaignProcessType.Create,
                    }
                };

                AuthenticationHelpers.SetupServiceRequest(request, "Create");

                var responseCreate = ServiceCallFactory.CampaignProcess_Create(request);
                var campaignProcess = responseCreate.Process;

                responseCreate.Process.Campaign = campaignGet;
                DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

                var model = PromotionFactory.CreateCampaignModel(
                    "Create promotion",
                    PageConst.Title_NavigateToHomePage,
                    Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                    campaignGet);

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
            else
            {
                TempData["Warning"] = "Promotion not found.";
                return View(Views.ViewNames.Index);
            }
        }

        public virtual ActionResult ContinuePromotion(int? id)
        {
            CreatePromotionModel model;
            if (id.HasValue)
            {
                var request = new GetCampaignProcessRequest
                {
                    Id = id.Value
                };

                AuthenticationHelpers.SetupServiceRequest(request, "Get");
                var response = ServiceCallFactory.CampaignProcess_Get(request);
                var campaign = (PromotionCampaign)response.Process.Campaign;
                var campaignProcess = response.Process;
                campaignProcess.Campaign = campaign;
                DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

                model = PromotionFactory.CreateCampaignModel(
                    "Create promotion",
                    PageConst.Title_NavigateToHomePage,
                    Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
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
            }
            else
            {
                model = Cache_Get_PromotionModel();
            }
            if (model == null) return RedirectToAction(ActionNameConstants.Index);
            return View(Views.ViewNames.Index, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult ContinueLater()
        {
            Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState.Draft);

            TempData["Info"] = "The promotion has been saved as 'draft'.";

            return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult ReadyToPublish()
        {
            var model = Cache_Get_PromotionModel();
            if (model.StatusList.ReadyToPublish)
            {
                Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState.Publish);

                TempData["Success"] = "The promotion has been saved and is 'ready to publish'.";
                return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
            }

            TempData["Danger"] = "The promotion isn't ready to publish.";
            return RedirectToAction(ActionNameConstants.ContinuePromotion);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemovePromotion(int id)
        {
            CreatePromotionModel model;
            var campaignProcess = CampaignFactory.GetCampaignProcess(id);
            var campaign = (PromotionCampaign)campaignProcess.Campaign;
            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            model = PromotionFactory.CreateCampaignModel("Remove promotion",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                campaign);
            model.Promotion.Id = campaignProcess.Id ?? 0;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemovePromotion(CreatePromotionModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Promotion.Id) as CampaignProcess;
            process.State = CampaignProcessState.Cancelled;
            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Create");
            var response = ServiceCallFactory.CampaignProcess_Create(request);

            TempData["Success"] = "The promotion has been removed.";

            return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
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

            campaignModel.StatusList = model;
            Cache_Set_PromotionModel(campaignModel);

            return PartialView(MVC.CreatePromotion.Views.DisplayTemplates.StatusListModel, model);
        }

        public virtual ActionResult SaveAsTemplate()
        {
            Cache_Set_Promotion_ProcessState_AndInsertOrUpdateRepositoryRecord(CampaignProcessState.Template);

            return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
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
            if (visibleFromDateTimeUtc > startDateTimeUtc)
                ModelState.AddModelError("VisibleFromDate", "Visible from must be before or on Start");
            if (model.StartDate < OperatingDateTime.TodayLocal)
                ModelState.AddModelError("StartDate", "Start date must be today or in future");
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
            if (campaign == null) return null;

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
                            ? MVC.CreatePromotion.Views.EditorTemplates.BasicStepModel
                            : MVC.CreatePromotion.Views.DisplayTemplates.BasicStepModel, model);
                    }
                case "SEGMENTATIONSTEP": // Step 2
                    {
                        model = (SegmentationStepModel)model ?? new SegmentationStepModel();
                        return PartialView(mode.ToUpper() == "EDIT"
                            ? MVC.CreatePromotion.Views.EditorTemplates.SegmentationStepModel
                            : MVC.CreatePromotion.Views.DisplayTemplates.SegmentationStepModel, model);
                    }
                case "TIMEFRAMESTEP": // Step 3
                    {
                        model = (PromotionTimeframeStepModel)model ?? new PromotionTimeframeStepModel();
                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.CreatePromotion.Views.EditorTemplates.TimeframeStepModel
                            : MVC.CreatePromotion.Views.DisplayTemplates.TimeframeStepModel, model);
                    }
                case "DISCOUNTSTEP": // Step 4
                    {
                        model = (DiscountStepModel)model ?? new DiscountStepModel();
                        var discounts = model as DiscountStepModel;
                        if (ModelState.IsValid ||
                            (!ModelState.IsValid && ModelState.Keys.Count == 1
                            && ModelState.Keys.FirstOrDefault(k => k == "DiscountRules") != null))
                        {
                            if (!ruleIndex.HasValue && discounts.RuleType == 0)
                            {
                                return PartialView(mode.ToUpper() == "EDIT"
                                    ? MVC.Shared.Views.EditorTemplates.DiscountStepModel
                                    : MVC.CreatePromotion.Views.DisplayTemplates.DiscountStepModel, model);
                            }
                        }

                        SetSelectedRule(discounts, ruleIndex);
                        return PartialView(MVC.CreatePromotion.Views.EditorTemplates.DiscountRuleModel, discounts);
                    }
                case "PROMOTIONSTEP": // Step 5
                    {
                        model = (PromotionStepModel)model ?? new PromotionStepModel();
                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.CreatePromotion.Views.EditorTemplates.PromotionStepModel
                            : MVC.CreatePromotion.Views.DisplayTemplates.PromotionStepModel, model);
                    }
                default:
                    return null;
            }
        }

        protected override string ReturnDiscountRuleModelEditorTemplateName()
        {
            return MVC.CreatePromotion.Views.EditorTemplates.DiscountRuleModel;
        }

        #endregion Step Getters

        #region Cache

        private string _CacheKey_PromotionModel
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();
                return $"CreatePromotionModel-{((session != null) ? session.SessionTicket : "")}";
            }
        }

        private string _CacheKey_PromotionProcess
        {
            get
            {
                var session = AuthenticationHelpers.GetSession();
                return $"CreatePromotionProcess-{((session != null) ? session.SessionTicket : "")}";
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
                process.State = processState.Value;

            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            process = response.Process;
            process.Campaign = campaignBase;

            DistributedCache.Instance.SetObject(_CacheKey_PromotionProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override CreatePromotionModel Cache_Get_PromotionModel()
        {
            var model = DistributedCache.Instance.GetObject<CreatePromotionModel>(_CacheKey_PromotionModel);
            return model;
        }

        protected override void Cache_Set_PromotionModel(CreatePromotionModel model)
        {
            DistributedCache.Instance.SetObject(_CacheKey_PromotionModel, model, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        #endregion Cache
    }
}