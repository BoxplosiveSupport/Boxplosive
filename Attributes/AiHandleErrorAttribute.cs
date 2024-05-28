using Microsoft.ApplicationInsights;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
    public class AiHandleErrorAttribute : HandleErrorAttribute
    {
        /// <remarks>
        /// @see https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-exceptions#reuse-your-telemetry-client
        /// </remarks>
        private static readonly TelemetryClient _TelemetryClient = new TelemetryClient();

        /// <remarks>
        /// @see https://github.com/AppInsightsSamples/Mvc5UnhandledExceptionTelemetry/blob/master/Mvc5UnhandledExceptionTelemetry/ErrorHandler/AiHandleErrorAttribute.cs
        /// @see https://learn.microsoft.com/en-us/azure/azure-monitor/app/asp-net-exceptions#mvc-4-mvc-5
        /// </remarks>
        public override void OnException(ExceptionContext filterContext)
        {
            if (filterContext != null && filterContext.HttpContext != null && filterContext.Exception != null)
            {
                // If customError is Off, then AI HTTPModule will report the exception
                // If it is On, or RemoteOnly (default) - then we need to explicitly track the exception
                if (filterContext.HttpContext.IsCustomErrorEnabled)
                    _TelemetryClient.TrackException(filterContext.Exception);
            }

            base.OnException(filterContext);
        }
    }
}