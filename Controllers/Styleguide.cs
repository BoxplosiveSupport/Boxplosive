using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Models.Styleguide;
using System.IO;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public partial class StyleguideController : ControllerBase
    {
        // GET: Home
        public virtual ActionResult Index()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Styleguide"
            };

            return View(model);
        }

        public virtual ActionResult CreateCampaign()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Create Campaign"
            };

            return View(model);
        }

        public virtual ActionResult CreateLoyalty()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Create Loyalty"
            };

            return View(model);
        }

        public virtual ActionResult PublishCampaign()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Publish Campaign"
            };

            return View(model);
        }

        public virtual ActionResult Batches()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Batches"
            };

            return View(model);
        }

        public virtual ActionResult Batch()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Batch"
            };

            return View(model);
        }

        public virtual ActionResult BatchRetract()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Retract Batch"
            };

            return View(model);
        }

        public virtual ActionResult Analyse()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Analyse"
            };

            return View(model);
        }

        public virtual ActionResult Reclamation()
        {
            var model = new StyleguideModel
            {
                PageTitle = "Reclamation"
            };

            return View(model);
        }
    }
}