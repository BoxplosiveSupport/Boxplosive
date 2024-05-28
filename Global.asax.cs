using Microsoft.ApplicationInsights.Extensibility;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Binders;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace nl.boxplosive.BackOffice.Mvc
{
    public class Global : HttpApplication
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        private static readonly string _VersionNumber = AssemblyHelper.GetFileVersion();

        private void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            _Logger.Info("Application_Start " + _VersionNumber);

            // Note: Avoid using nl.boxplosive.Configuration AppConfig.Settings.* here because initialization can cause issues
            var appInsightsConnnectionString = ConfigurationManager.AppSettings.Get("APPLICATIONINSIGHTS_CONNECTION_STRING");
            if (!string.IsNullOrWhiteSpace(appInsightsConnnectionString))
            {
                TelemetryConfiguration.Active.ConnectionString = appInsightsConnnectionString;
            }

            // Code that runs on application startup
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            Configuration.Context.TrySetStore(new HttpContextStore());

            // HACK: Override default 'claims' principal selector,
            // otherwise it will become faulty after changing to 'boxplosive' principal and back
            ClaimsPrincipal.ClaimsPrincipalSelector = () =>
            {
                ClaimsPrincipal principal = HttpContext.Current.User as ClaimsPrincipal;
                if (principal != null)
                {
                    return principal;
                }

                return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim> { new Claim(ClaimTypes.PrimarySid, Guid.NewGuid().ToString()) }));
            };

            // Configure unique 'claim' identifier, required for correct working of 'AntiForgery'
            AntiForgeryConfig.UniqueClaimTypeIdentifier = ClaimTypes.PrimarySid;

            ModelBinders.Binders.Add(typeof(DateTime), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeModelBinder());
            ModelBinders.Binders.Add(typeof(double), new DoubleModelBinder());
            ModelBinders.Binders.Add(typeof(double?), new DoubleModelBinder());
            ModelBinders.Binders.Add(typeof(HtmlEditor), new HtmlEditorModelBinder());
            ModelBinders.Binders.Add(typeof(PlaceholderModel), new PlaceholderModelBinder());

            DataAnnotationsModelValidatorProvider.RegisterAdapter(typeof(ToStringLengthAttribute), typeof(ToStringLengthAdapter));

            // Set BackOffice Version
            Application["BackOfficeVersion"] = _VersionNumber;
        }

        private void Application_End(object sender, EventArgs e)
        {
            // Code that runs on application shutdown
            _Logger.Info("Application_End " + _VersionNumber);
        }

        private void Application_PreSendRequestHeaders()
        {
            Response.Headers.Remove("Server");
            Response.Headers.Remove("X-Powered-By");
            Response.Headers.Remove("X-AspNet-Version");
            Response.Headers.Remove("X-AspNetMvc-Version");
        }

        private void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        private void Application_EndRequest(object sender, EventArgs e)
        {
        }

        private void Session_Start(object sender, EventArgs e)
        {
        }

        private void Session_End(object sender, EventArgs e)
        {
        }
    }
}