using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.PublishLoyalty;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Utilities.Caching;
using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
    public partial class PublishLoyaltyController : LoyaltyControllerBase<PublishLoyaltyModel>
    {
        #region LoyaltyControllerBase overrides

        private string _CacheKey_LoyaltyCampaign
        {
            get
            {
                Session session = AuthenticationHelpers.GetSession();
                return string.Format("PublishLoyalty-LoyaltyCampaign-{0}", (session != null) ? session.SessionTicket : "");
            }
        }

        protected override LoyaltyCampaign GetLoyaltyFromCache()
        {
            return DistributedCache.Instance.GetObject<LoyaltyCampaign>(_CacheKey_LoyaltyCampaign);
        }

        protected override void SaveLoyaltyInCache(LoyaltyCampaign campaign)
        {
            DistributedCache.Instance.SetObject(_CacheKey_LoyaltyCampaign, campaign, CacheSettings.DefaultRelativeExpiration_Backoffice);
        }

        protected override PublishLoyaltyModel GetLoyaltyModelFromCache()
        {
            throw new NotImplementedException();
        }

        protected override void StoreLoyaltyModelInCache(PublishLoyaltyModel model)
        {
            throw new NotImplementedException();
        }

        #endregion LoyaltyControllerBase overrides

        public virtual ActionResult ReadyToPublish(int id)
        {
            var process = CampaignFactory.GetCampaignProcess(id);
            if (process.Type == CampaignProcessType.Publish)
            {
                TempData["Danger"] = "The loyalty is already published. To edit this loyalty go to Publications and modify the loyalty.";
                return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
            }

            var campaign = (LoyaltyCampaign)process.Campaign;
            SaveLoyaltyInCache(campaign);

            var model = LoyaltyFactory.CreatePublishCampaignModel(
                "Publish loyalty",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                campaign);
            model.Loyalty.Id = process.Id ?? 0;

            ClearModelStateAndTryValidateModels(
                new List<StepModelBase> { model.Loyalty.Basics, model.Loyalty.Timeframe, model.Loyalty.Rewards, model.Loyalty.Savings });

            if (ModelState.IsValid)
                return View(model);

            TempData["Danger"] = "The loyalty isn't ready to publish.";
            return Edit(model);
        }

        [HttpPost]
        [MultipleSubmitButtons(Name = "action", Argument = "Edit")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(PublishLoyaltyModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Loyalty.Id);
            if (process.Type == CampaignProcessType.Publish)
            {
                TempData["Danger"] = "The loyalty is already published. To edit this loyalty go to Publications and modify the loyalty.";
                return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
            }

            process.State = CampaignProcessState.Draft;
            var request = new CampaignProcessRequest
            {
                Process = process as CampaignProcess
            };

            AuthenticationHelpers.SetupServiceRequest(request, "Create");
            var response = ServiceCallFactory.CampaignProcess_Create(request);
            if (response.Success)
            {
                return RedirectToAction(MVC.CreateLoyalty.ActionNames.ContinueLoyalty, MVC.CreateLoyalty.Name,
                    new { id = model.Loyalty.Id });
            }

            TempData["Warning"] = response.Message;
            return RedirectToAction(ActionNameConstants.ReadyToPublish, new { id = model.Loyalty.Id });
        }

        [HttpPost]
        [MultipleSubmitButtons(Name = "action", Argument = "Publish")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Publish(PublishLoyaltyModel model)
        {
            var result = PublishLoyalty(model);

            TempData["Success"] = $"The loyalty has been published with id: {result.CampaignId}";

            return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
        }

        private PublishCampaignProcessResponse PublishLoyalty(PublishLoyaltyModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Loyalty.Id);
            var campaign = (LoyaltyCampaign)process.Campaign;

            var requestContract = LoyaltyFactory.ConvertCampaignToPublishRequest(campaign, process as CampaignProcess);
            return ServiceCallFactory.CampaignProcess_Publish(requestContract);
        }
    }
}