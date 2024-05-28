using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.PublishPromotion;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ProcessContract.CampaignProcessService.DataContracts;
using nl.boxplosive.Service.ProcessModel;
using nl.boxplosive.Service.ServiceModel;
using System;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [Attributes.Authorize(Roles = "ApplicationManager, CampaignManager, DataEntryEmployee, DataEntryManager")]
    public partial class PublishPromotionController : ControllerBase
    {
        public virtual ActionResult ReadyToPublish(int id)
        {
            var process = CampaignFactory.GetCampaignProcess(id);
            var promotionCampaign = (PromotionCampaign)process.Campaign;
            var model = PromotionFactory.CreatePublishCampaignModel(
                "Publish promotion",
                PageConst.Title_NavigateToHomePage,
                Url.Action(HomeController.ActionNameConstants.Index, HomeController.NameConst), "",
                promotionCampaign);
            model.Promotion.Id = process.Id.HasValue ? process.Id.Value : 0;

            model.Promotion.Basics.IsValid = TryValidateModel(model.Promotion.Basics);
            model.Promotion.Segmentation.IsValid = TryValidateModel(model.Promotion.Segmentation);
            model.Promotion.Timeframe.IsValid = TryValidateModel(model.Promotion.Timeframe);
            model.Promotion.Discount.IsValid = TryValidateModel(model.Promotion.Discount);
            model.Promotion.Promotion.IsValid = TryValidateModel(model.Promotion.Promotion);

            if (ModelState.IsValid)
                return View(model);

            TempData["Danger"] = "The promotion isn't ready to publish.";
            return Edit(model);
        }

        [HttpPost]
        [MultipleSubmitButtons(Name = "action", Argument = "Edit")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(PublishPromotionModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Promotion.Id);
            process.State = CampaignProcessState.Draft;

            var request = new CampaignProcessRequest
            {
                Process = process as CampaignProcess
            };

            switch (process.Type)
            {
                case CampaignProcessType.Create:
                    {
                        AuthenticationHelpers.SetupServiceRequest(request, "Create");
                        var response = ServiceCallFactory.CampaignProcess_Create(request);

                        if (response.Success)
                        {
                            return RedirectToAction(MVC.CreatePromotion.ActionNames.ContinuePromotion, MVC.CreatePromotion.Name, new { id = model.Promotion.Id });
                        }

                        TempData["Warning"] = response.Message;
                        return RedirectToAction(ActionNameConstants.ReadyToPublish, new { id = model.Promotion.Id });
                    }

                case CampaignProcessType.Modify:
                    {
                        AuthenticationHelpers.SetupServiceRequest(request, "Modify");
                        var response = ServiceCallFactory.CampaignProcess_Modify(request);

                        if (response.Success)
                        {
                            return RedirectToAction(MVC.ModifyPromotion.ActionNames.Index, MVC.ModifyPromotion.Name, new { id = model.Promotion.Id });
                        }

                        TempData["Warning"] = response.Message;
                        return RedirectToAction(ActionNameConstants.ReadyToPublish, new { id = model.Promotion.Id });
                    }
                default:
                    TempData["Warning"] = String.Format("Process type {0} is not supported for editing", process.Type);
                    return RedirectToAction(ActionNameConstants.ReadyToPublish, new { id = model.Promotion.Id });
            }
        }

        [HttpPost]
        [MultipleSubmitButtons(Name = "action", Argument = "Publish")]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Publish(PublishPromotionModel model)
        {
            var result = PublishPromotion(model);

            TempData["Success"] = String.Format("The promotion has been published with id: {0}", result.CampaignId);

            return RedirectToAction(MVC.Home.ActionNames.Index, MVC.Home.Name);
        }

        private PublishCampaignProcessResponse PublishPromotion(PublishPromotionModel model)
        {
            var process = CampaignFactory.GetCampaignProcess(model.Promotion.Id);
            var campaign = (PromotionCampaign)process.Campaign;

            var requestContract = PromotionFactory.ConvertCampaignToPublishRequest(campaign, process);
            return ServiceCallFactory.CampaignProcess_Publish(requestContract);
        }
    }
}