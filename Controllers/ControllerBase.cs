using Microsoft.Owin.Security;
using nl.boxplosive.BackOffice.Business.Business.StartupMessage;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.BackOffice.Mvc.Utils;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Exceptions;
using nl.boxplosive.Business.Sdk.Template;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using AuthorizeAttribute = nl.boxplosive.BackOffice.Mvc.Attributes.AuthorizeAttribute;

namespace nl.boxplosive.BackOffice.Mvc.Controllers
{
    public abstract class ControllerBase : Controller
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        protected ControllerBase()
        {
        }

        private IList<string> _AuthorizedRoles;

        public IList<string> AuthorizedRoles
        {
            get
            {
                if (_AuthorizedRoles == null)
                {
                    var type = GetType();

                    _AuthorizedRoles = type.GetCustomAttributes<AuthorizeAttribute>().SelectMany(o => o.AuthorizedRoles).ToList();

                    if (_AuthorizedRoles.Count == 0)
                    {
                        if (_Logger.IsWarnEnabled)
                        {
                            _Logger.Warn($"No authorization roles or attribute {typeof(AuthorizeAttribute)} on type={type}");
                        }
                    }
                }

                return _AuthorizedRoles;
            }
        }

        protected HttpStatusCodeResult Forbidden()
        {
            return new HttpStatusCodeResult(HttpStatusCode.Forbidden);
        }

        protected ViewResult ViewAndMessage(object model, UiMessage message)
        {
            message.SetMessageInTempData(TempData);

            return View(model);
        }

        protected IList<string> GetModelStateErrorKeys()
        {
            return ModelState.Where(ms => ms.Value.Errors.Count > 0).Select(ms => ms.Key).ToList();
        }

        protected bool HasModelStateErrors(string key)
        {
            return GetModelStateErrorKeys().Contains(key);
        }

        protected void ClearModelStateAndTryValidateModels(ICollection<StepModelBase> models)
        {
            ModelState.Clear();
            foreach (StepModelBase model in models)
            {
                model.IsValid = TryValidateModel(model);
                ModelState.Clear();
            }
        }

        protected bool ClearModelStateAndTryValidateModel(object model, ICollection<string> modelStateKeysToValidate)
        {
            ModelState.Clear();
            TryValidateModel(model);

            var result = true;
            foreach (string key in modelStateKeysToValidate)
            {
                if (ModelState.TryGetValue(key, out ModelState value))
                {
                    result = result && !value.Errors.Any();
                }
            }

            ModelState.Clear();

            return result;
        }

        protected override void OnException(ExceptionContext filterContext)
        {
            Exception ex = filterContext.Exception;

            // logout user when session is expired or not exists anymore (and redirect to login page)
            if (ex is SessionExpiredException || ex is SessionNotFoundException)
            {
                _Logger.Info(ex, "Session invalid, sign out user");

                ActionResult actionResultOverride = RedirectToRoute("Default", new
                {
                    action = MVC.Account.ActionNames.Login,
                    controller = MVC.Account.Name,
                    returnUrl = filterContext.HttpContext.Request.RawUrl,
                });

                filterContext.Result = SignOutUser(actionResultOverride);
                filterContext.ExceptionHandled = true;

                return;
            }

            _Logger.Fatal(ex, $"Unhandled application error in call: {ex.Message}");

            if (filterContext.ExceptionHandled || !filterContext.HttpContext.IsCustomErrorEnabled)
            {
                return;
            }

            // if the request is AJAX return JSON else view.
            if (IsAjax(filterContext))
            {
                // because its a exception raised after ajax invocation
                // lets return Json
                filterContext.Result = new JsonResult()
                {
                    Data = filterContext.Exception.Message,
                    JsonRequestBehavior = JsonRequestBehavior.AllowGet
                };

                filterContext.ExceptionHandled = true;
                filterContext.HttpContext.Response.Clear();

                return;
            }

            // Output a nice error page
            filterContext.ExceptionHandled = true;
            var viewResult = new ViewResult()
            {
                ViewName = MVC.Shared.Views.ViewNames.Error,
            };
            filterContext.Result = viewResult;
        }

        private bool IsAjax(ExceptionContext filterContext)
        {
            return filterContext.HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
        }

        #region Authentication

        public virtual ActionResult SignOutUser(ActionResult actionResultOverride = null)
        {
            if (Globals.B2C_Enabled)
            {
                if (Request.IsAuthenticated)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    MsalAppBuilder.ClearUserTokenCache();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    IEnumerable<AuthenticationDescription> authTypes = HttpContext.GetOwinContext().Authentication.GetAuthenticationTypes();
                    HttpContext.GetOwinContext().Authentication.SignOut(authTypes.Select(t => t.AuthenticationType).ToArray());
                    Request.GetOwinContext().Authentication.GetAuthenticationTypes();
                }

                return actionResultOverride != null ? actionResultOverride : RedirectToAction("Index", "Home");
            }

            IAuthenticationManager authenticationManager = HttpContext.GetOwinContext().Authentication;
            authenticationManager.SignOut();

            string wsLogoutUrl = AppConfig.Settings.WebSealLogoutUrl;
            if (!string.IsNullOrEmpty(wsLogoutUrl))
            {
                string uriLeftPart = new Uri(Request.Url.AbsoluteUri).GetLeftPart(UriPartial.Authority);
                string redirectUrl = new Uri(new Uri(uriLeftPart), wsLogoutUrl).ToString();

                return Redirect(redirectUrl);
            }

            return actionResultOverride != null ? actionResultOverride : RedirectToAction("Index", "Home");
        }

        #endregion Authentication

        #region API

        protected ICampaignApi CampaignApi => BusinessApiFactory.GetInstance().BusinessApi<ICampaignApi>();
        protected ICustomerApi CustomerApi => BusinessApiFactory.GetInstance().BusinessApi<ICustomerApi>();
        protected ILabelInternalApi LabelInternalApi => BusinessApiFactory.GetInstance().BusinessApi<ILabelInternalApi>();
        protected ILoyaltyEventDefinitionApi LoyaltyEventDefinitionApi => BusinessApiFactory.GetInstance().BusinessApi<ILoyaltyEventDefinitionApi>();
        protected IProductApi ProductApi => BusinessApiFactory.GetInstance().BusinessApi<IProductApi>();
        protected IStartupMessageApi StartupMessageApi => BusinessApiFactory.GetInstance().BusinessApi<IStartupMessageApi>();
        protected ITemplateApi TemplateApi => BusinessApiFactory.GetInstance().BusinessApi<ITemplateApi>();

        #endregion API

        #region Repositories

        protected static IAppCardStackDefinitionRepository AppCardStackDefinitionRepository =>
            DataRepositoryFactory.GetInstance().DataRepository<IAppCardStackDefinitionRepository>();
        protected static IEdmBatchRepository EdmBatchRepository => DataRepositoryFactory.GetInstance().DataRepository<IEdmBatchRepository>();
        protected static INotificationDefinitionRepository NotificationDefinitionRepository =>
            DataRepositoryFactory.GetInstance().DataRepository<INotificationDefinitionRepository>();
        protected static IPlaceholderRepository PlaceholderRepository => DataRepositoryFactory.GetInstance().DataRepository<IPlaceholderRepository>();
        protected static ITemplateRepository TemplateRepository => DataRepositoryFactory.GetInstance().DataRepository<ITemplateRepository>();

        #endregion Repositories
    }
}