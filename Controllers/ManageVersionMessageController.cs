using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Mapping;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.ManageVersionMessage;
using nl.boxplosive.Utilities.Json;
using System;
using System.Linq;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    [FeatureAttribute("StartupMessage", Order = 0)]
    [Attributes.Authorize(Roles = "ApplicationManager", Order = 1)]
    public partial class ManageVersionMessageController : PlatformController
    {
        private static readonly string _ManageStartupMessageTitle = "Manage Startup Message";

        public virtual ActionResult Index()
        {
            var model = CreateVersionsMessageItemsModel();
            model.VersionMessageItems = StartupMessageApi.GetVersionMessages().Select(v => v.Map()).ToList();
            return View(model);
        }

        public virtual ActionResult Edit(string version = null)
        {
            var model = CreateVersionMessageModel(version);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Edit(VersionMessageModel model)
        {
            if (model.New)
            {
                if (StartupMessageApi.GetVersionMessages(model.AppVersion).Any())
                {
                    TempData["Danger"] = $"Version {model.AppVersion} is already defined, please enter another version.";
                    ModelState.AddModelError(nameof(model.AppVersion), TempData["Danger"].ToString());
                }
            }

            if (!JsonWrapper.IsValidJObject(model.AlertMessage))
                ModelState.AddModelError(nameof(model.AlertMessage), ErrorMessageInvalidJsonFormat);

            if (!JsonWrapper.IsValidJObject(model.ErrorMessage))
                ModelState.AddModelError(nameof(model.ErrorMessage), ErrorMessageInvalidJsonFormat);

            if (!ModelState.IsValid)
                return View(CreateVersionMessageModel(model));

            var message = model.Map();
            StartupMessageApi.MergeVersionMessages(new[] { message });

            return RedirectToAction(MVC.ManageVersionMessage.ActionNames.Index);
        }

        public virtual ActionResult Delete(string version = null)
        {
            var model = CreateVersionMessageModel(version);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult Delete(VersionMessageModel model)
        {
            StartupMessageApi.DeleteVersionMessage(model.AppVersion);
            return RedirectToAction(MVC.ManageVersionMessage.ActionNames.Index);
        }

        private VersionMessageItemsModel CreateVersionsMessageItemsModel()
        {
            return new VersionMessageItemsModel
            {
                PageTitle = _ManageStartupMessageTitle,
                PageNavigationUrl = Url.Action(MVC.Home.ActionNames.Index, MVC.Home.Name),
                PageNavigationUrlText = PageConst.Title_NavigateToHomePage,
            };
        }

        private VersionMessageModel CreateVersionMessageModel(VersionMessageModel model)
        {
            model.PageTitle = _ManageStartupMessageTitle;
            model.PageNavigationUrl = Url.Action(MVC.ManageVersionMessage.ActionNames.Index, MVC.ManageVersionMessage.Name);
            model.PageNavigationUrlText = PageConst.Title_NavigateToPreviousPage;
            return model;
        }

        private VersionMessageModel CreateVersionMessageModel(string version = null)
        {
            var model = new VersionMessageModel
            {
                PageTitle = _ManageStartupMessageTitle,
                PageNavigationUrl = Url.Action(MVC.ManageVersionMessage.ActionNames.Index, MVC.ManageVersionMessage.Name),
                PageNavigationUrlText = PageConst.Title_NavigateToPreviousPage,
                New = true
            };

            if (!String.IsNullOrEmpty(version))
            {
                var message = StartupMessageApi.GetVersionMessages(version).SingleOrDefault();
                if (message != null)
                {
                    model = message.Map(model);
                }
            }

            return model;
        }
    }
}