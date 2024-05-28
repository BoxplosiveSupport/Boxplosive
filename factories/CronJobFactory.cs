using System;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Management;
using nl.boxplosive.Service.ServiceContract.CronJobService.DataContracts;

namespace nl.boxplosive.BackOffice.Mvc.Factories
{
    public class CronJobFactory
    {
        public static BackgroundProcessesModel CreateBackgroundProcessesModel(string pageTitle,
            string pageNavigationLabel,
            string pageNavigationUrl,
            UrlHelper url)
        {
            var model = new BackgroundProcessesModel
            {
                PageTitle = pageTitle,
                PageNavigationUrl = pageNavigationUrl,
                PageNavigationUrlText = pageNavigationLabel,
            };

            var request = new GetAllCronJobRequest();
            AuthenticationHelpers.SetupServiceRequest(request, "GetAll");
            var response = ServiceCallFactory.CronJob_GetAll(request);
            if (response.Success)
            {
                model.BackgroundProcesses = response.CronJobs.Select(cj => new BackgroundProcessModel(cj, url)).ToList();
            }

            return model;
        }
    }
}