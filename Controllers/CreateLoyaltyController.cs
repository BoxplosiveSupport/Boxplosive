using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.CreateLoyalty;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceContract.CampaignService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Service.ServiceModel.Campaign;
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
    public partial class CreateLoyaltyController : LoyaltyControllerBase<CreateLoyaltyModel>
    {
        // GET: CreateCampaign
        public virtual ActionResult Index()
        {
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
            var loyaltyCampaign = new LoyaltyCampaign
            {
                StartDate = OperatingDateTime.TodayUtc,
                EndDate = OperatingDateTime.EndOfDayUtc(TimeSpan.FromDays(7)).FloorByMinute(),
                Rewards = new List<RewardBase>(),
                Savings = new List<Saving>(),
                IsAutomaticallySubscribed = false,
                IsPublic = true,
            };
            campaignProcess.Campaign = loyaltyCampaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            var model = LoyaltyFactory.CreateCampaignModel(
                "Create loyalty",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                loyaltyCampaign);

            StoreLoyaltyModelInCache(model);
            return View(model);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult Copy(int id)
        {
            // Grab the correct campaign from the ID
            var copyrequest = new GetCampaignRequest
            {
                Id = id.ToString(),
                Type = CampaignEntityType.Loyalty
            };
            AuthenticationHelpers.SetupServiceRequest(copyrequest, "Get");
            var copyresponse = ServiceCallFactory.Campaign_Get(copyrequest);
            if (copyresponse.Success)
            {
                // Set id's for rewards and savings
                var campaign = (LoyaltyCampaign)copyresponse.Campaign;
                campaign.IsCopy = true;
                campaign.Id = -1;
                campaign.PlaceholderValues.ForEach(item => item.Id = 0);

                var count = -1;
                var discountCount = 1;
                campaign.Rewards.ForEach(r =>
                {
                    r.Id = count--;
                    r.PlaceholderValues.ForEach(item => item.Id = 0);
                    if (r is ClaimReward)
                    {
                        var claimReward = (ClaimReward)r;
                        claimReward.Discount.Id = discountCount++;
                        claimReward.CouponCode = string.Empty;
                    }
                });

                count = 1;
                campaign.Savings.ForEach(s => s.Discount.Id = count++);
                campaign.Savings.ForEach(s => s.CouponCode = string.Empty);

                // Set the DateTimes as UTC
                campaign.StartDate = new DateTime(campaign.StartDate.Ticks, DateTimeKind.Utc);
                campaign.EndDate = new DateTime(campaign.EndDate.Ticks, DateTimeKind.Utc);

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
                campaignProcess.Campaign = campaign;
                DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

                var model = LoyaltyFactory.CreateCampaignModel(
                    "Create loyalty",
                    PageConst.Title_NavigateToHomePage,
                    Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                    campaign);

                ValidateLoyaltyModelStates(model.Loyalty);
                model.StatusList.ValidateLoyaltyAndSetStepStatuses(model.Loyalty);

                StoreLoyaltyModelInCache(model);

                return View(Views.ViewNames.Index, model);
            }
            else
            {
                TempData["Warning"] = "Promotion not found.";
                return View(Views.ViewNames.Index);
            }
        }

        public virtual ActionResult ContinueLoyalty(int? id)
        {
            CreateLoyaltyModel model;
            if (id.HasValue)
            {
                var campaignProcess = CampaignFactory.GetCampaignProcess(id.Value);
                if (campaignProcess.Type == CampaignProcessType.Publish)
                {
                    TempData["Danger"] = "The loyalty is already published. To edit this loyalty go to Publications and modify the loyalty.";
                    return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
                }

                var campaign = (LoyaltyCampaign)campaignProcess.Campaign;
                DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

                model = LoyaltyFactory.CreateCampaignModel("Create loyalty",
                    PageConst.Title_NavigateToHomePage,
                    Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                    campaign);

                ValidateLoyaltyModelStates(model.Loyalty);
                model.StatusList.ValidateLoyaltyAndSetStepStatuses(model.Loyalty);

                StoreLoyaltyModelInCache(model);
            }
            else
            {
                model = GetLoyaltyModelFromCache();
            }

            if (model == null) return RedirectToAction(ActionNameConstants.Index);
            return View(Views.ViewNames.Index, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult ContinueLater()
        {
            SaveCachedLoyaltyAsDraft();

            TempData["Info"] = "The loyalty has been saved as 'draft'.";

            return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult ReadyToPublish()
        {
            var model = GetLoyaltyModelFromCache();
            if (model.StatusList.ReadyToPublish)
            {
                SaveCachedLoyaltyAsReadyToPublish();

                TempData["Success"] = "The loyalty has been saved and is 'ready to publish'.";
                return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
            }

            TempData["Danger"] = "The loyalty isn't ready to publish.";
            return RedirectToAction(ActionNameConstants.ContinueLoyalty);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemoveLoyalty(int id)
        {
            CreateLoyaltyModel model;
            var campaignProcess = CampaignFactory.GetCampaignProcess(id);
            var campaign = (LoyaltyCampaign)campaignProcess.Campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            model = LoyaltyFactory.CreateCampaignModel("Remove loyalty",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                campaign);
            model.Loyalty.Id = campaignProcess.Id ?? 0;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemoveLoyalty(CreateLoyaltyModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Loyalty.Id) as CampaignProcess;
            process.State = CampaignProcessState.Cancelled;
            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Create");
            var response = ServiceCallFactory.CampaignProcess_Create(request);

            TempData["Success"] = "The loyalty has been removed.";

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
            var campaignModel = GetLoyaltyModelFromCache();

            ValidateLoyaltyModelStates(campaignModel.Loyalty);
            model.ValidateLoyaltyAndSetStepStatuses(campaignModel.Loyalty);
            campaignModel.StatusList = model;

            StoreLoyaltyModelInCache(campaignModel);

            return PartialView(MVC.CreateLoyalty.Views.DisplayTemplates.StatusListModel, model);
        }

        public virtual ActionResult SaveAsTemplate()
        {
            SaveCachedLoyaltyAsTemplate();

            return RedirectToAction(HomeController.ActionNameConstants.Index, HomeController.NameConst);
        }

        #region Step Setters

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepBasics(BasicStepModel model, string id, string mode)
        {
            bool modelIsValid = ValidateBalanceMessages(model);
            modelIsValid = ModelState.IsValid && modelIsValid;
            if (!modelIsValid)
                return ReturnCorrectView(id, mode, model);

            var campaign = GetLoyaltyFromCache();
            model.CopyValuesToServiceModel(campaign);

            if (campaign.IsContinuousSaving && campaign.CombineRewards)
            {
                model.IsValid = false;
                ModelState.AddModelError("IsContinuousSaving", "Continuous saving is not allowed when combine rewards is enabled");
            }

            SaveLoyaltyInCache(campaign);

            model = RetrieveModelFromCache(id) as BasicStepModel;
            var campaignModel = GetLoyaltyModelFromCache();
            campaignModel.Loyalty.Basics = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            StoreLoyaltyModelInCache(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepTimeframe(TimeframeStepModel model, string id, string mode)
        {
            var campaign = GetLoyaltyFromCache();

            var startDateTimeUtc = new DateTime(model.StartDate.Ticks + model.StartTime.Ticks, DateTimeKind.Local).ToUtcInOperatingTimeZone();
            var endDateTimeUtc = new DateTime(model.EndDate.Ticks + model.EndTime.Ticks, DateTimeKind.Local).ToUtcInOperatingTimeZone();
            if (model.StartDate < OperatingDateTime.TodayLocal)
                ModelState.AddModelError("StartDate", "Start date must be today or in future");
            if (endDateTimeUtc < startDateTimeUtc)
                ModelState.AddModelError("EndDate", "End must be after start");

            campaign.StartDate = startDateTimeUtc;
            campaign.EndDate = endDateTimeUtc;

            // FIX BPD-796 If Start/Enddates are equal set to 24h
            if (campaign.StartDate.Equals(campaign.EndDate))
            {
                campaign.EndDate = campaign.EndDate.AddDays(1);
            }

            // TODO: nothing for margin

            // ETOS-1418 (2015-12-01): changed requirement.
            // A least one reward should be end on or after the loyalty program
            // The 'maxRewardsEndsAt' is in UTC, the model values are in local, so needs to be converted
            if (campaign.Rewards != null && campaign.Rewards.Count > 0)
            {
                DateTime maxRewardsEndsAt = campaign.Rewards.Max(r => r.EndsAt);
                if (maxRewardsEndsAt.ToLocalTimeInOperatingTimeZone().Subtract(model.EndDate).TotalMilliseconds < -1.0)
                {
                    ModelState.AddModelError("EndDate", "End date should be at most the max reward's end date");
                }
            }

            SaveLoyaltyInCache(campaign);

            model = RetrieveModelFromCache(id) as TimeframeStepModel;
            var campaignModel = GetLoyaltyModelFromCache();
            campaignModel.Loyalty.Timeframe = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            StoreLoyaltyModelInCache(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public virtual ActionResult SetStepRewards(RewardsStepModel model, string id, string mode, int? ruleIndex, string actionType = null)
        {
            DiscountRuleType ruletype = model.RuleType;
            RewardsRuleModel rule = model.SelectedRule;
            List<RewardsRuleModel> rules = model.RewardRules;
            bool combineRewards = model.CombineRewards == true;
            bool rewardActivation = model.RewardActivation;

            // Direct validation to prevent side effects like saving the discount when canceled
            if (rule != null)
            {
                bool isSubsequentCombinedReward = rule.IsSubsequentCombinedReward == true;

                bool hasDirectModelStateError = false;
                foreach (string modelStateKey in RewardsRuleModel.GetModelStateValidationKeys(isSubsequentCombinedReward))
                {
                    hasDirectModelStateError = hasDirectModelStateError || HasModelStateErrors($"SelectedRule.{modelStateKey}");
                }

                if (rule.MatchMoreUnits && rule.ApplicableAmount != rule.RequiredAmount)
                {
                    ModelState.AddModelError("SelectedRule.ApplicableAmount",
                        "When match more units is checked, No. of times to apply must be equal to the required amount");
                    hasDirectModelStateError = true;
                }

                if (!ValidateRedemptionPeriod(rule.RedemptionPeriod))
                {
                    ModelState.AddModelError("SelectedRule.RedemptionPeriod", "The redemption period in days or timespan must be greater than zero.");
                    hasDirectModelStateError = true;
                }

                if (hasDirectModelStateError)
                {
                    return ReturnCorrectView(id, mode, model, ruleIndex, actionType);
                }
            }

            var campaignModel = GetLoyaltyModelFromCache();
            if (rule != null)
            {
                AssignProductsToRewardRule(rule, campaignModel.Loyalty, combineRewards, ruleIndex);
            }

            var cachedModel = RetrieveModelFromCache(id) as RewardsStepModel;
            cachedModel.RuleType = ruletype;
            cachedModel.CombineRewards = combineRewards;
            cachedModel.RewardActivation = rewardActivation;

            if (combineRewards)
            {
                CopySharedFieldsForCombinedRewards(cachedModel);
            }

            // Set the removed rule
            if (actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                cachedModel.RewardRules[ruleIndex.Value].Remove = true;
                ruleIndex = null;
            }

            if (rule != null)
            {
                if (rule.Id == 0)
                {
                    var ruleId = cachedModel.RewardRules.Count > 0 ? cachedModel.RewardRules.Min(sr => sr.Id) - 1 : -1;
                    if (ruleId >= 0) ruleId = -1;
                    rule.Id = ruleId;
                    rule.New = true;
                    cachedModel.RewardRules.Add(rule);
                }
                else
                {
                    if (combineRewards)
                    {
                        cachedModel.RewardRules[ruleIndex.Value] = rule;
                    }
                    else
                    {
                        var srIndex = cachedModel.RewardRules.Select((s, index) => new { index, s })
                            .First(sr => sr.s.Id == rule.Id)
                            .index;
                        cachedModel.RewardRules[srIndex] = rule;
                    }
                }

                cachedModel.SelectedRule = null;

                campaignModel.Loyalty.Rewards = cachedModel;
                StoreLoyaltyModelInCache(campaignModel);
            }

            // Update the draft cache
            var campaign = LoyaltyFactory.UpdateCampaignRewards(GetLoyaltyFromCache(), cachedModel);
            SaveLoyaltyInCache(campaign);

            // Recollect model from cache for error messages
            cachedModel = RetrieveModelFromCache(id) as RewardsStepModel;

            // Revalidate the model
            ValidateRewardsStepModelStates(cachedModel);
            cachedModel.RuleType = ruletype;
            cachedModel.CombineRewards = combineRewards;
            cachedModel.RewardActivation = rewardActivation;
            campaignModel.Loyalty.Rewards = cachedModel;

            ValidateRewardRulesWithLoyaltyCampaign(rule, cachedModel);

            // Collect the correct view
            var viewResult = ReturnCorrectView(id, mode, cachedModel, ruleIndex, actionType);

            bool isEmptyRule = cachedModel.SelectedRule != null && cachedModel.SelectedRule.Id == 0;
            if (isEmptyRule)
            {
                DateTime todayLocal = OperatingDateTime.TodayLocal;
                bool todayLocalAfterLoyaltyProgramStart = todayLocal > campaignModel.Loyalty.Timeframe.StartDate.Add(campaignModel.Loyalty.Timeframe.StartTime);
                cachedModel.SelectedRule.StartsAtDate = todayLocalAfterLoyaltyProgramStart ? todayLocal.Date : campaignModel.Loyalty.Timeframe.StartDate;
                cachedModel.SelectedRule.StartsAtTime = todayLocalAfterLoyaltyProgramStart ? todayLocal.TimeOfDay : campaignModel.Loyalty.Timeframe.StartTime;
                cachedModel.SelectedRule.EndsAtDate = campaignModel.Loyalty.Timeframe.EndDate;
                cachedModel.SelectedRule.EndsAtTime = campaignModel.Loyalty.Timeframe.EndTime;
            }

            // Reset the selected RuleType
            campaignModel.Loyalty.Rewards.RuleType = 0;
            StoreLoyaltyModelInCache(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult SetStepSavings(SavingsStepModel model, string id, string mode, int? ruleIndex = null, string actionType = null)
        {
            var campaignModel = GetLoyaltyModelFromCache();
            if (model.SelectedRule != null)
            {
                ValidateSavingStep(model);

                var cachedSelectedRule = campaignModel.Loyalty.Savings.SavingRules.FirstOrDefault(r => r.Id == model.SelectedRule.Id) ?? new SavingsRuleModel();
                var modelSelectedRule = model.SelectedRule ?? new SavingsRuleModel();
                LoadProductsFromMultipleProductUpload(modelSelectedRule.SelectedProducts, cachedSelectedRule.SelectedProducts, "SelectedProducts");
            }

            if (model.SelectedRule != null && model.SelectedRule.ErrorMessages.Count > 0)
            {
                return PartialView(MVC.CreateLoyalty.Views.EditorTemplates.SavingsStepModel, model);
            }

            int? anonymousBalanceLimit = model.AnonymousBalanceLimit;
            SavingsDisplayTypeModel SavingsDisplayType = model.SavingsDisplayType;
            var rule = model.SelectedRule;
            var ruletype = model.RuleType;
            var rules = model.SavingRules;
            model = RetrieveModelFromCache(id) as SavingsStepModel;
            model.AnonymousBalanceLimit = anonymousBalanceLimit;
            model.SavingsDisplayType = SavingsDisplayType;
            model.RuleType = ruletype;

            // Set the removed rule
            if (actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                model.SavingRules[ruleIndex.Value].Remove = true;
                ruleIndex = null;
            }

            // Insert/Update the new rule
            if (rule != null)
            {
                //model = campaignModel.Loyalty.Savings;

                if (rule.Id == 0)
                {
                    var ruleId = model.SavingRules.Count > 0 ? model.SavingRules.Min(sr => sr.Id) - 1 : -1;
                    if (ruleId >= 0) ruleId = -1;
                    rule.Id = ruleId;
                    rule.New = true;
                    model.SavingRules.Add(rule);
                }
                else
                {
                    var srIndex = model.SavingRules.Select((s, index) => new { index, s })
                        .First(sr => sr.s.Id == rule.Id)
                        .index;
                    model.SavingRules[srIndex] = rule;
                }

                model.SelectedRule = null;

                campaignModel.Loyalty.Savings = model;
                StoreLoyaltyModelInCache(campaignModel);
            }

            // Update the draft cache
            LoyaltyCampaign campaign = LoyaltyFactory.UpdateLoyaltyCampaignSavings(GetLoyaltyFromCache(), model);
            SaveLoyaltyInCache(campaign);

            // Recollect model from cache for error messages
            model = RetrieveModelFromCache(id) as SavingsStepModel;

            // Revalidate the model
            ValidateSavingsStepModelStates(model);
            model.AnonymousBalanceLimit = anonymousBalanceLimit;
            model.SavingsDisplayType = SavingsDisplayType;
            model.RuleType = ruletype;
            campaignModel.Loyalty.Savings = model;

            if (model.SavingRules.Any(m => m.ErrorMessages.Count > 0))
            {
                ModelState.AddModelError("ErrorMessages", "");
            }

            // Collect the correct view
            var viewResult = ReturnCorrectView(id, mode, model);

            // Reset the selected RuleType
            campaignModel.Loyalty.Savings.RuleType = null;

            StoreLoyaltyModelInCache(campaignModel);

            return viewResult;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UploadImageRewardRule()
        {
            return UploadRewardImage();
        }

        #endregion Step Setters

        #region Step Getters

        private StepModelBase RetrieveModelFromCache(string id)
        {
            var campaign = GetLoyaltyFromCache();
            if (campaign == null) return null;

            switch (id.ToUpper())
            {
                case "BASICSTEP": // Step 1
                    {
                        return new BasicStepModel(campaign);
                    }

                case "TIMEFRAMESTEP": // Step 2
                    {
                        var model = new TimeframeStepModel
                        {
                            StartDate = campaign.StartDate.ToLocalTimeInOperatingTimeZone().Date,
                            EndDate = campaign.EndDate.ToLocalTimeInOperatingTimeZone().Date,
                            StartTime = campaign.StartDate.ToLocalTimeInOperatingTimeZone().TimeOfDay,
                            EndTime = campaign.EndDate.ToLocalTimeInOperatingTimeZone().TimeOfDay
                        };
                        return model;
                    }

                case "REWARDSSTEP":
                    {
                        return LoyaltyFactory.CreateRewardsStepModel(campaign);
                    }

                case "SAVINGSSTEP":
                    {
                        return LoyaltyFactory.CreateSavingsStepModel(campaign);
                    }

                default:
                    return null;
            }
        }

        private ActionResult ReturnCorrectView(string id, string mode, StepModelBase model, int? ruleIndex = null, string actionType = null)
        {
            if (model != null)
            {
                model.IsValid = model.IsValid ?? ModelState.IsValid;
                model.ErrorFieldNames.AddRange(GetModelStateErrorKeys());
            }

            switch (id.ToUpper())
            {
                case "BASICSTEP": // Step 1
                    {
                        model = (BasicStepModel)model ?? new BasicStepModel();
                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.CreateLoyalty.Views.EditorTemplates.BasicStepModel
                            : MVC.CreateLoyalty.Views.DisplayTemplates.BasicStepModel, model);
                    }

                case "TIMEFRAMESTEP": // Step 2
                    {
                        model = (TimeframeStepModel)model ?? new TimeframeStepModel();
                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.CreateLoyalty.Views.EditorTemplates.TimeframeStepModel
                            : MVC.CreateLoyalty.Views.DisplayTemplates.TimeframeStepModel, model);
                    }

                case "REWARDSSTEP":
                    {
                        bool isDeleteActionType = actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase);

                        bool showRewardsRule = mode.ToUpper() == "EDIT"
                            // the 'delete' action not exists on the 'Views.EditorTemplates.RewardsRuleModel', only on 'Views.EditorTemplates.RewardsStepModel'
                            && !isDeleteActionType
                            && (!ModelState.IsValid
                                    || Request.Form.AllKeys.Contains("RuleType")
                                    || ((Request.QueryString.AllKeys.Contains("ruleIndex") || Request.Form.AllKeys.Contains("ruleIndex"))
                                            && !Request.Form.AllKeys.Any(key => key.StartsWith("SelectedRule."))));
                        if (showRewardsRule)
                            return ReturnCorrectRewardsRuleView((RewardsStepModel)model, ruleIndex);

                        return PartialView(mode.ToUpper() == "EDIT"
                            ? MVC.CreateLoyalty.Views.EditorTemplates.RewardsStepModel
                            : MVC.CreateLoyalty.Views.DisplayTemplates.RewardsStepModel, model);
                    }

                case "SAVINGSSTEP":
                    {
                        model = (SavingsStepModel)model ?? new SavingsStepModel();
                        var savings = model as SavingsStepModel;
                        if (!ruleIndex.HasValue && !savings.RuleType.HasValue)
                        {
                            return PartialView(mode.ToUpper() == "EDIT"
                                ? MVC.CreateLoyalty.Views.EditorTemplates.SavingsStepModel
                                : MVC.CreateLoyalty.Views.DisplayTemplates.SavingsStepModel, model);
                        }

                        return ReturnCorrectSavingsRuleView(savings, ruleIndex);
                    }

                default:
                    return null;
            }
        }

        private ActionResult ReturnCorrectSavingsRuleView(SavingsStepModel savings, int? ruleIndex)
        {
            SavingsRuleTypeModel type = ruleIndex.HasValue
                ? savings.SavingRules.ElementAt(ruleIndex.Value).Type
                : savings.RuleType.Value;
            SavingsRuleModel ruleModel = ruleIndex.HasValue
                ? savings.SavingRules.ElementAt(ruleIndex.Value)
                : new SavingsRuleModel { Type = type, Id = 0 };

            // If the selected rule is a new savings rule, set initial values
            if (!ruleIndex.HasValue)
            {
                var loyaltyModel = GetLoyaltyModelFromCache();
                DateTime todayLocal = OperatingDateTime.TodayLocal;
                bool todayLocalAfterLoyaltyProgramStart = todayLocal > loyaltyModel.Loyalty.Timeframe.StartDate.Add(loyaltyModel.Loyalty.Timeframe.StartTime);
                ruleModel.StartDate = todayLocalAfterLoyaltyProgramStart ? todayLocal.Date : loyaltyModel.Loyalty.Timeframe.StartDate;
                ruleModel.From = todayLocalAfterLoyaltyProgramStart ? todayLocal.TimeOfDay : loyaltyModel.Loyalty.Timeframe.StartTime;
                ruleModel.EndDate = loyaltyModel.Loyalty.Timeframe.EndDate;
                ruleModel.To = loyaltyModel.Loyalty.Timeframe.EndTime;
            }

            savings.SelectedRule = ruleModel;
            savings.SelectedRule.Index = ruleIndex;

            return PartialView(MVC.Loyalty.Views.SavingRuleTypeTemplates.SavingsRuleTemplate, savings);
        }

        private ActionResult ReturnCorrectRewardsRuleView(RewardsStepModel rewards, int? ruleIndex)
        {
            if (rewards.SelectedRule == null)
            {
                DiscountRuleType type = ruleIndex.HasValue
                    ? rewards.RewardRules.ElementAt(ruleIndex.Value).Type
                    : rewards.RuleType;
                RewardsRuleModel ruleModel = ruleIndex.HasValue
                    ? rewards.RewardRules.ElementAt(ruleIndex.Value)
                    : new RewardsRuleModel(null, rewards.CombineRewards) { Type = type, Id = 0 };
                rewards.SelectedRule = ruleModel;
            }

            rewards.SelectedRule.Index = rewards.SelectedRule.Index ?? ruleIndex;

            SetViewData_IsContinuousSaving();

            return PartialView(MVC.CreateLoyalty.Views.EditorTemplates.RewardsRuleModel, rewards);
        }

        #endregion Step Getters

        #region Cache

        private string _CacheKey_LoyaltyModel
        {
            get
            {
                Session session = AuthenticationHelpers.GetSession();
                return string.Format("LoyaltyModel-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        private string _CacheKey_LoyaltyProcess
        {
            get
            {
                Session session = AuthenticationHelpers.GetSession();
                return string.Format("LoyaltyProcess-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        protected override LoyaltyCampaign GetLoyaltyFromCache()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            return process != null ? process.Campaign as LoyaltyCampaign : null;
        }

        protected override CreateLoyaltyModel GetLoyaltyModelFromCache()
        {
            var model = DistributedCache.Instance.GetObject<CreateLoyaltyModel>(_CacheKey_LoyaltyModel);
            return model;
        }

        protected override void SaveLoyaltyInCache(LoyaltyCampaign campaign)
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            process.Campaign = campaign;
            var request = new CampaignProcessRequest
            {
                Process = process
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override void StoreLoyaltyModelInCache(CreateLoyaltyModel model)
        {
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyModel, model, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        private void SaveCachedLoyaltyAsDraft()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            var campaign = process.Campaign;
            process.State = CampaignProcessState.Draft;
            var request = new CampaignProcessRequest
            {
                Process = process
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        private void SaveCachedLoyaltyAsTemplate()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            var campaign = process.Campaign;
            process.State = CampaignProcessState.Template;
            var request = new CampaignProcessRequest
            {
                Process = process
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        private void SaveCachedLoyaltyAsReadyToPublish()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            var campaign = process.Campaign;
            process.State = CampaignProcessState.Publish;
            var request = new CampaignProcessRequest
            {
                Process = process
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Create");

            var response = ServiceCallFactory.CampaignProcess_Create(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        #endregion Cache
    }
}