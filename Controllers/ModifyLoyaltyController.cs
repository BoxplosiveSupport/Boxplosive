using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Management;
using nl.boxplosive.BackOffice.Mvc.Models.ModifyLoyalty;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceContract.CampaignService.DataContracts;
using nl.boxplosive.Service.ServiceContract.CustomerService.DataContracts;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Campaign;
using nl.boxplosive.Utilities.Caching;
using nl.boxplosive.Utilities.Date;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using WebGrease.Css.Extensions;
using CampaignEntityType = nl.boxplosive.Service.ServiceModel.Campaign.CampaignEntityType;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager, ReclamationEmployee, ReclamationManager")]
    public partial class ModifyLoyaltyController : LoyaltyControllerBase<ModifyLoyaltyModel>
    {
        // GET: ModifyLoyalty
        public virtual ActionResult Index(int id)
        {
            ActionResult actionResult = GetActionResult(setReferrerAbsoluteUri: true, out bool redirectToHome);
            if (id < 1)
            {
                TempData["Warning"] = "The given Loyalty couldn't be found!";

                return RedirectToAction(actionResult);
            }

            ModifyLoyaltyModel model;

            var campaignProcess = CampaignFactory.GetCampaignProcess(id);
            var campaign = (LoyaltyCampaign)campaignProcess.Campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            model = LoyaltyFactory.CreateModifyCampaignModel(
                "Modify loyalty",
                redirectToHome ? PageConst.Title_NavigateToHomePage : PageConst.Title_NavigateToManagementPublicationsPage,
                Url.Action(actionResult),
                string.Empty,
                campaign);

            ValidateLoyaltyModelStates(model.Loyalty);
            model.StatusList.ValidateLoyaltyAndSetStepStatuses(model.Loyalty);

            StoreLoyaltyModelInCache(model);

            return View(model);
        }

        public virtual ActionResult Modify(int id)
        {
            var returnId = 0;

            // Grab the correct campaign from the ID
            var request = new GetCampaignRequest
            {
                Id = id.ToString(),
                Type = CampaignEntityType.Loyalty
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Get");
            var response = ServiceCallFactory.Campaign_Get(request);
            if (response.Success)
            {
                // Set id's for rewards and savings
                var campaign = (LoyaltyCampaign)response.Campaign;
                var count = 1;
                campaign.Rewards.Where(r => r is ClaimReward)
                    .ForEach(r => ((ClaimReward)r).Discount.Id = count++);
                count = 1;
                campaign.Savings.ForEach(s => s.Discount.Id = count++);

                // Set the DateTimes as UTC
                campaign.StartDate = new DateTime(campaign.StartDate.Ticks, DateTimeKind.Utc);
                campaign.EndDate = new DateTime(campaign.EndDate.Ticks, DateTimeKind.Utc);

                // Generate a new Modify draft
                var modifyRequest = new CampaignProcessRequest
                {
                    Process = new CampaignProcess
                    {
                        State = CampaignProcessState.Created,
                        Type = CampaignProcessType.Modify,
                        Campaign = campaign
                    }
                };
                AuthenticationHelpers.SetupServiceRequest(modifyRequest, "Modify");
                var modifyResponse = ServiceCallFactory.CampaignProcess_Modify(modifyRequest);

                // Redirect to the index with the new draft id
                if (modifyResponse.Success)
                {
                    returnId = modifyResponse.Process.Id.Value;
                }
            }

            return RedirectToAction(MVC.ModifyLoyalty.Index(returnId));
        }

        // GET: ModifyLoyalty/Customers
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult Customers([Bind(Prefix = "id")] int loyaltyProgramId)
        {
            IDtoLoyaltyPointsProgram loyalty = GetLoyaltyAndClearCustomerUploadData(loyaltyProgramId);

            return View(new ModifyLoyaltyCustomersModel(loyalty)
            {
                EnabledBalancesCount = LoyaltyPointsBalanceRepository.GetCountEnabled(loyalty)
            });
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Customers(ModifyLoyaltyCustomersModel model)
        {
            var selectedCustomers = (List<string>)TempData["CustomerUploadData-" + model.Id];

            IDtoLoyaltyPointsProgram loyalty = LoyaltyPointsProgramRepository.Get(model.Id);
            if (!model.IsPublic && (selectedCustomers == null || selectedCustomers.Count == 0))
            {
                ModelState.AddModelError("SelectedCustomers", "No customers are selected.");

                return View(new ModifyLoyaltyCustomersModel(loyalty));
            }

            if (model.IsPublic != loyalty.IsPublic || model.IsAutomaticallySubscribed != loyalty.IsAutomaticallySubscribed)
            {
                loyalty.IsPublic = model.IsPublic;
                loyalty.IsAutomaticallySubscribed = model.IsAutomaticallySubscribed;

                LoyaltyPointsProgramRepository.Update(loyalty);
            }

            if (!model.IsPublic)
            {
                var customerIds =
                    CustomerProfileRepository
                        .GetCustomerIdsBySearchValues(DataConfig.Instance.CustomerProfileExternalIdKey, new HashSet<string>(selectedCustomers))
                        .Values;

                LoyaltyPointsBalanceRepository.MergeEnabledCustomers(loyalty, new HashSet<int>(customerIds));
            }

            TempData["Success"] = $"Customers are configured for loyalty: {loyalty.Name} ({loyalty.Id}).";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        // GET: ModifyLoyalty/GrantLoyaltyPoints
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        public virtual ActionResult GrantLoyaltyPoints([Bind(Prefix = "id")] int loyaltyProgramId)
        {
            IDtoLoyaltyPointsProgram loyalty = GetLoyaltyAndClearCustomerUploadData(loyaltyProgramId);

            return View(new ModifyLoyaltyGrantLoyaltyPointsModel(loyalty));
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult GrantLoyaltyPoints(ModifyLoyaltyGrantLoyaltyPointsModel model)
        {
            string customerUploadDataId = "CustomerUploadData-" + model.Id;
            var selectedCustomers = (List<string>)TempData.Peek(customerUploadDataId);
            IDtoLoyaltyPointsProgram loyalty = LoyaltyPointsProgramRepository.Get(model.Id);

            bool error = false;
            if (selectedCustomers == null || selectedCustomers.Count == 0)
            {
                ModelState.AddModelError("SelectedCustomers", "No customers are selected.");
                error = true;
            }

            int maxRewardThreshold = loyalty.Rewards.Max(r => r.Threshold);
            if (model.NumberOfLoyaltyPoints < 1 || model.NumberOfLoyaltyPoints > maxRewardThreshold)
            {
                model.SelectedCustomers = selectedCustomers == null ? 0 : selectedCustomers.Count();

                var validationMessage = $"The number of loyalty points should be less than or equal to {maxRewardThreshold}";

                ModelState.AddModelError("NumberOfLoyaltyPoints", validationMessage);
                error = true;
            }

            if (error)
            {
                return View(model);
            }

            ILoyaltyPointsApi loyaltyPointsApi = BusinessApiInjector.GetInstance<ILoyaltyPointsApi>();
            loyaltyPointsApi.CreateGrantLoyaltyPointsEvent(model.Id, new HashSet<string>(selectedCustomers), model.NumberOfLoyaltyPoints);

            TempData.Remove(customerUploadDataId);
            TempData["Success"] = "GrantLoyaltyPoints event added.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpPost]
        public virtual JsonResult CustomerUpload([Bind(Prefix = "id")] int loyaltyProgramId)
        {
            HttpPostedFileBase httpPostedFileBase = Request.Files.Get(0);

            var lines = new List<string>();
            using (var streamReader = new StreamReader(httpPostedFileBase.InputStream))
            {
                string line;
                while ((line = streamReader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            var request = new GetCustomersWithValidationRequest
            {
                ExternalIds = lines,
            };
            AuthenticationHelpers.SetupServiceRequest(request, "GetMultipleWithValidation");
            GetCustomersWithValidationResponse response = ServiceCallFactory.Customer_GetMultipleWithValidation(request);

            // Note: that JSON content length should be really compact (by default the limit is 4 MB)
            // @see https://msdn.microsoft.com/en-us/library/system.web.script.serialization.javascriptserializer.maxjsonlength.aspx
            var selected = new List<object[]>();
            var errors = new List<object[]>();
            var tempData = new List<string>();
            foreach (CustomerValidation customerValidation in response.CustomerValidations)
            {
                string errorType = null;
                if (customerValidation.Occurrences > 1)
                {
                    errorType = "D";
                    int duplicateCount = customerValidation.Occurrences - 1;
                    errors.Add(new object[3]
                    {
                        customerValidation.ExternalId,
                        errorType,
                        duplicateCount,
                    });
                }

                if (customerValidation.CustomerExists)
                {
                    selected.Add(new object[2]
                    {
                        customerValidation.Customer.Id,
                        customerValidation.Customer.ExternalId,
                    });
                    tempData.Add(customerValidation.Customer.ExternalId);
                }
                else
                {
                    errorType = "U";
                    errors.Add(new object[3]
                    {
                        customerValidation.ExternalId,
                        errorType,
                        customerValidation.Occurrences
                    });
                }
            }

            var result = new
            {
                MultiItemFileName = httpPostedFileBase.FileName,
                MultiItemCount = lines.Count,
                // Only set selected when there are no validation errors
                MultiItemSelected = errors.Any() ? new List<object[]>() : selected,
                MultiItemNewCount = 0,
                MultiItemErrorCount = errors.Count,
                MultiItemErrors = errors,
                // Do not show individual items, show the count
                MultiItemDisplayThreshold = -1,
            };

            TempData["CustomerUploadTempData-" + loyaltyProgramId] = result.MultiItemErrorCount == 0 ? tempData : new List<string>();

            return new JsonResult()
            {
                Data = result,
                JsonRequestBehavior = JsonRequestBehavior.AllowGet,
                MaxJsonLength = int.MaxValue
            };
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpPost]
        public virtual JsonResult ConfirmCustomerUpload([Bind(Prefix = "id")] int loyaltyProgramId)
        {
            string tempDataKey = "CustomerUploadTempData-" + loyaltyProgramId;
            var tempData = (List<string>)TempData[tempDataKey];
            if (tempData != null)
            {
                TempData.Remove(tempDataKey);
                TempData["CustomerUploadData-" + loyaltyProgramId] = tempData;

                return Json(tempData.Count);
            }

            return Json(0);
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager")]
        [HttpGet]
        public virtual FileResult CustomerDownload([Bind(Prefix = "id")] int loyaltyProgramId)
        {
            IEnumerable<IDtoLoyaltyPointsBalance> balances = LoyaltyPointsBalanceRepository.GetAllEnabled(new LazyDtoLoyaltyPointsProgram(loyaltyProgramId));

            return File(
                balances.SelectMany(b => Encoding.UTF8.GetBytes(b.Customer.ExternalId + Environment.NewLine)).ToArray(),
                "text/csv",
                $"LoyaltyCustomers-{loyaltyProgramId}-{balances.Count()}.csv"
            );
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Cancel()
        {
            // Delete the draft
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
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
            SaveCachedLoyaltyAsDraft();

            TempData["Info"] = "The loyalty has been saved as 'draft'.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult Publish()
        {
            if (PublishCachedLoyalty())
                TempData["Success"] = "The loyalty has been published.";
            else
                TempData["Danger"] = "The loyalty hasn't been published.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        private bool PublishCachedLoyalty()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);
            var campaign = process.Campaign as LoyaltyCampaign;
            process.State = CampaignProcessState.Publish;

            var requestContract = LoyaltyFactory.ConvertCampaignToPublishRequest(campaign, process);
            var publishresponse = ServiceCallFactory.CampaignProcess_Publish(requestContract);

            return publishresponse.Success;
        }

        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemoveLoyalty(int id)
        {
            ModifyLoyaltyModel model;
            var campaignProcess = CampaignFactory.GetCampaignProcess(id);
            var campaign = (LoyaltyCampaign)campaignProcess.Campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, campaignProcess, CacheSettings.DefaultRelativeExpiration_Backoffice);

            ActionResult actionResult = GetActionResult(setReferrerAbsoluteUri: false, out bool redirectToHome);
            model = LoyaltyFactory.CreateModifyCampaignModel(
                "Remove loyalty",
                redirectToHome ? PageConst.Title_NavigateToHomePage : PageConst.Title_NavigateToManagementPublicationsPage,
                Url.Action(actionResult),
                string.Empty,
                campaign);
            model.Loyalty.Id = campaignProcess.Id ?? 0;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
        public virtual ActionResult RemoveLoyalty(ModifyLoyaltyModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Loyalty.Id) as CampaignProcess;
            process.State = CampaignProcessState.Cancelled;
            var request = new CampaignProcessRequest
            {
                Process = process
            };
            AuthenticationHelpers.SetupServiceRequest(request, "Create");
            var response = ServiceCallFactory.CampaignProcess_Modify(request);

            TempData["Success"] = "The loyalty has been removed.";

            return RedirectToAction(GetActionResult(setReferrerAbsoluteUri: false, out _));
        }

        public virtual ActionResult GetStatusList()
        {
            var model = new ModifyStatusListModel();
            var campaignModel = GetLoyaltyModelFromCache();
            model.LoyaltyProgramId = campaignModel.LoyaltyProgramId;

            ValidateLoyaltyModelStates(campaignModel.Loyalty);
            model.ValidateLoyaltyAndSetStepStatuses(campaignModel.Loyalty);
            campaignModel.StatusList = model;

            StoreLoyaltyModelInCache(campaignModel);

            return PartialView(MVC.ModifyLoyalty.Views.DisplayTemplates.ModifyStatusListModel, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult UploadImageRewardRule()
        {
            return UploadRewardImage();
        }

        private IDtoLoyaltyPointsProgram GetLoyaltyAndClearCustomerUploadData(int loyaltyProgramId)
        {
            IDtoLoyaltyPointsProgram loyalty = LoyaltyPointsProgramRepository.Get(loyaltyProgramId);
            string customerUploadDataId = "CustomerUploadData-" + loyalty.Id;
            TempData.Remove(customerUploadDataId);

            return loyalty;
        }

        #region Steps

        public virtual ActionResult GetStepPartial(string id, string mode, int? ruleIndex)
        {
            var model = RetrieveModelFromCache(id);

            return ReturnCorrectView(id, mode, model, ruleIndex);
        }

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
            var minStartDateTimeUtc = campaign.StartDate.ToUtcBeginOfDayInOperatingTimeZone() < OperatingDateTime.TodayUtc
                ? campaign.StartDate.ToUtcBeginOfDayInOperatingTimeZone()
                : OperatingDateTime.TodayUtc;
            if (startDateTimeUtc < minStartDateTimeUtc)
                ModelState.AddModelError("StartDate", $"Start must be on or after {minStartDateTimeUtc.ToLocalTimeInOperatingTimeZone().ToString("g")}");
            if (endDateTimeUtc < startDateTimeUtc)
                ModelState.AddModelError("EndDate", "End must be after start");

            // ETOS-1418 (2015-12-01): changed requirement.
            // A least one reward should be end on or after the loyalty program
            // The 'maxRewardsEndsAt' is in UTC, the model values are in local, so needs to be converted

            if (campaign.Rewards != null && campaign.Rewards.Count > 0)
            {
                DateTime maxRewardsEndsAt = campaign.Rewards.Max(r => r.EndsAt);

                if (maxRewardsEndsAt.ToLocalTimeInOperatingTimeZone().Subtract(model.EndDate).TotalMilliseconds < -1.0)
                {
                    ModelState.AddModelError("EndDate", "End date cannot be greater than the reward end date, change the reward end date first.");
                }
            }

            // ETOS-1456: Only allow StartDate change if loyalty has not started yet
            campaign.StartDate = startDateTimeUtc;
            campaign.EndDate = endDateTimeUtc;

            // FIX BPD-796 If Start/Enddates are equal set to 24h
            if (campaign.StartDate.Equals(campaign.EndDate))
            {
                campaign.EndDate = campaign.EndDate.AddDays(1);
            }

            // TODO: nothing for margin

            SaveLoyaltyInCache(campaign);

            model = RetrieveModelFromCache(id) as TimeframeStepModel;
            var campaagnModel = GetLoyaltyModelFromCache();
            campaagnModel.Loyalty.Timeframe = model;

            var viewResult = ReturnCorrectView(id, mode, model);

            StoreLoyaltyModelInCache(campaagnModel);

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

            // Set the disabled/removed rule
            if (actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                cachedModel.RewardRules[ruleIndex.Value].Disable = true;
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
                return PartialView(MVC.ModifyLoyalty.Views.EditorTemplates.SavingsStepModel, model);
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

            // Set the disabled/removed rule
            if (actionType != null && actionType.Equals("delete", StringComparison.OrdinalIgnoreCase))
            {
                var modelSavingRule = model.SavingRules[ruleIndex.Value];
                modelSavingRule.Disable = true;
                if (string.IsNullOrEmpty(modelSavingRule.CouponCode))
                    modelSavingRule.Remove = true;

                ruleIndex = null;
            }

            // Insert/Update the new rule
            if (rule != null)
            {
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

        #endregion Steps

        #region Step Getters

        private StepModelBase RetrieveModelFromCache(string id)
        {
            var campaign = GetLoyaltyFromCache();
            if (campaign == null)
                return null;

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

                case "REWARDSSTEP": // Step 3
                    {
                        var rewardStepModel = LoyaltyFactory.CreateRewardsStepModel(campaign);

                        return rewardStepModel;
                    }

                case "SAVINGSSTEP": // Step 4
                    {
                        var savingsStepModel = LoyaltyFactory.CreateSavingsStepModel(campaign);
                        savingsStepModel.IsPublished = true;

                        return savingsStepModel;
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
                            ? MVC.ModifyLoyalty.Views.EditorTemplates.BasicStepModel
                            : MVC.ModifyLoyalty.Views.DisplayTemplates.BasicStepModel, model);
                    }

                case "TIMEFRAMESTEP": // Step 2
                    {
                        model = (TimeframeStepModel)model ?? new TimeframeStepModel();

                        return PartialView(mode.ToUpper() == "EDIT" || !ModelState.IsValid
                            ? MVC.ModifyLoyalty.Views.EditorTemplates.TimeframeStepModel
                            : MVC.ModifyLoyalty.Views.DisplayTemplates.TimeframeStepModel, model);
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
                            ? MVC.ModifyLoyalty.Views.EditorTemplates.RewardsStepModel
                            : MVC.ModifyLoyalty.Views.DisplayTemplates.RewardsStepModel, model);
                    }

                case "SAVINGSSTEP": // Step 4
                    {
                        model = (SavingsStepModel)model ?? new SavingsStepModel()
                        {
                            IsPublished = true
                        };

                        var savings = model as SavingsStepModel;
                        if (!ruleIndex.HasValue && !savings.RuleType.HasValue)
                        {
                            return PartialView(mode.ToUpper() == "EDIT"
                                ? MVC.ModifyLoyalty.Views.EditorTemplates.SavingsStepModel
                                : MVC.ModifyLoyalty.Views.DisplayTemplates.SavingsStepModel, model);
                        }

                        return ReturnCorrectSavingsRuleView(savings, ruleIndex);
                    }

                default:
                    return null;
            }
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

            return PartialView(MVC.ModifyLoyalty.Views.EditorTemplates.RewardsRuleModel, rewards);
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

        #endregion Step Getters

        #region Cache

        private string _CacheKey_LoyaltyModel
        {
            get
            {
                Service.ServiceModel.Authentication.Session session = AuthenticationHelpers.GetSession();

                return string.Format("ModifyLoyaltyModel-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        private string _CacheKey_LoyaltyProcess
        {
            get
            {
                Service.ServiceModel.Authentication.Session session = AuthenticationHelpers.GetSession();

                return string.Format("ModifyLoyaltyProcess-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        protected override LoyaltyCampaign GetLoyaltyFromCache()
        {
            var process = DistributedCache.Instance.GetObject<CampaignProcess>(_CacheKey_LoyaltyProcess);

            return process != null ? process.Campaign as LoyaltyCampaign : null;
        }

        protected override ModifyLoyaltyModel GetLoyaltyModelFromCache()
        {
            var model = DistributedCache.Instance.GetObject<ModifyLoyaltyModel>(_CacheKey_LoyaltyModel);

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

            AuthenticationHelpers.SetupServiceRequest(request, "Modify");

            var response = ServiceCallFactory.CampaignProcess_Modify(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override void StoreLoyaltyModelInCache(ModifyLoyaltyModel model)
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

            AuthenticationHelpers.SetupServiceRequest(request, "Modify");

            var response = ServiceCallFactory.CampaignProcess_Modify(request);
            process = response.Process;
            process.Campaign = campaign;
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyProcess, process, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        #endregion Cache

        #region Helpers

        private static readonly string _SessionKey_ReferrerAbsoluteUri = $"{nameof(ModifyLoyaltyController)}_ReferrerAbsoluteUri";

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

                return MVC.Management.Publications(page: null, PublicationsType.Loyalties);
            }

            redirectToHome = true;

            return MVC.Home.Index();
        }

        #endregion Helpers

        #region Repositories

        protected ICustomerProfileRepository CustomerProfileRepository =
            DataRepositoryFactory.GetInstance().DataRepository<ICustomerProfileRepository>();
        protected ILoyaltyPointsBalanceRepository LoyaltyPointsBalanceRepository =
            DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyPointsBalanceRepository>();
        protected ILoyaltyPointsProgramRepository LoyaltyPointsProgramRepository =
            DataRepositoryFactory.GetInstance().DataRepository<ILoyaltyPointsProgramRepository>();

        #endregion Repositories
    }
}
