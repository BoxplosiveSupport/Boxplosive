using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Utils;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceContract.IssuerService.DataContracts;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class AuthorizeAttribute : System.Web.Mvc.AuthorizeAttribute
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        private IList<string> _authorizedRoles;

        public IList<string> AuthorizedRoles
        {
            get
            {
                if (_authorizedRoles == null)
                {
                    _authorizedRoles = new List<string>(SplitString(Roles));
                }

                return _authorizedRoles;
            }
        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (Globals.B2C_Enabled)
            {
                var result = base.AuthorizeCore(httpContext);
                return result;
            }

            if (!VerifyClaimsPrincipal() && !TryWebSealLogin(httpContext))
            {
                return false;
            }

            return base.AuthorizeCore(httpContext);
        }

        /// <summary>
        /// Specifies that access to a controller or action method is restricted to users who meet the authorization requirement, Results in a 403 if unauthorized.
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/238437/why-does-authorizeattribute-redirect-to-the-login-page-for-authentication-and-au
        /// The AuthorizeAttribute redirect to the login page for authentication and authorization failures. When it was first developed, System.Web.Mvc.AuthorizeAttribute was doing the right thing - older revisions of the HTTP specification used status code 401 for both "unauthorized" and "unauthenticated".
        /// From the original specification:
        /// - If the request already included Authorization credentials, then the 401 response indicates that authorization has been refused for those credentials.
        /// In fact, you can see the confusion right there - it uses the word "authorization" when it means "authentication". In everyday practice, however, it makes more sense to return a 403 Forbidden when the user is authenticated but not authorized. It's unlikely the user would have a second set of credentials that would give them access - bad user experience all around.
        /// Consider most operating systems - when you attempt to read a file you don't have permission to access, you aren't shown a login screen!
        /// Thankfully, the HTTP specifications were updated (June 2014) to remove the ambiguity.
        /// From "Hyper Text Transport Protocol (HTTP/1.1): Authentication" (RFC 7235):
        /// - The 401 (Unauthorized) status code indicates that the request has not been applied because it lacks valid authentication credentials for the target resource.
        /// From "Hypertext Transfer Protocol (HTTP/1.1): Semantics and Content" (RFC 7231):
        /// - The 403 (Forbidden) status code indicates that the server understood the request but refuses to authorize it.
        /// Interestingly enough, at the time ASP.NET MVC 1 was released the behavior of AuthorizeAttribute was correct. Now, the behavior is incorrect - the HTTP/1.1 specification was fixed.
        /// Rather than attempt to change ASP.NET's login page redirects, it's easier just to fix the problem at the source. You can create a new attribute with the same name (AuthorizeAttribute) in your website's default namespace (this is very important) then the compiler will automatically pick it up instead of MVC's standard one. Of course, you could always give the attribute a new name if you'd rather take that approach.
        /// </remarks>
        protected override void HandleUnauthorizedRequest(System.Web.Mvc.AuthorizationContext filterContext)
        {
            if (Globals.B2C_Enabled)
            {
                base.HandleUnauthorizedRequest(filterContext);
                return;
            }

            // If we are authenticated, but we are authorized, return a 403
            if (filterContext.HttpContext.Request.IsAuthenticated)
            {
                filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden);
                return;
            }

            // If not Ajax, go on as normal
            base.HandleUnauthorizedRequest(filterContext);

            // Check if the call is from Ajax
            if (filterContext.HttpContext.Request.IsAjaxRequest())
            {
                // Throw a Unauthorized result, Ajax will catch it
                filterContext.Result = new HttpStatusCodeResult((int)HttpStatusCode.Forbidden);
            }
        }

        #region Helpers

        private bool VerifyClaimsPrincipal()
        {
            // Make sure that Boxplosive session is not expired!
            // This is required because of sliding expiration (application cookie),
            // the expires can become higher than the Boxplosive session time span.
            Claim claim = ClaimsPrincipal.Current.FindFirst(ClaimTypes.Expiration);
            if (claim == null)
            {
                return false;
            }

            object sessionEndsAtObj = claim.Value.FromDatabaseStringByKind();
            if (sessionEndsAtObj == null)
            {
                return false;
            }

            DateTime sessionEndsAt = (DateTime)sessionEndsAtObj;
            if (DateTime.UtcNow >= sessionEndsAt)
            {
                return false;
            }

            return true;
        }

        private bool TryWebSealLogin(HttpContextBase httpContext)
        {
            HashSet<string> wsHostWhitelist = WebSealHostWhitelist;
            if (wsHostWhitelist.Count == 0)
            {
                if (_Logger.IsDebugEnabled)
                    _Logger.Debug("webseal unauthorized: no whitelist configured");

                return false;
            }

            string requestHostName = httpContext?.Request?.UserHostName;
            string requestIpAddress = httpContext?.Request?.UserHostAddress;
            if (!WebSealHostIsValid(requestHostName, requestIpAddress, wsHostWhitelist))
            {
                if (_Logger.IsInfoEnabled)
                    _Logger.Info($"webseal unauthorized: hostname {requestHostName} / ip address {requestIpAddress} not whitelisted");

                return false;
            }

            string wsUser = GetWebSealUser();
            if (string.IsNullOrWhiteSpace(wsUser))
            {
                if (_Logger.IsDebugEnabled)
                    _Logger.Debug("webseal unauthorized: no user");

                return false;
            }

            IList<string> wsGroups = GetWebSealGroups();
            if (wsGroups.Count == 0)
            {
                if (_Logger.IsDebugEnabled)
                    _Logger.Debug("webseal unauthorized: no groups");

                return false;
            }

            IList<string> permissionGroups = GetPermissionGroups(wsGroups);
            if (permissionGroups.Count == 0)
            {
                if (_Logger.IsDebugEnabled)
                    _Logger.Debug("webseal unauthorized: no permission groups");

                return false;
            }

            string defaultIssuerPermissionGroup = AppConfig.Settings.DefaultIssuerPermissionGroup;
            if (!permissionGroups.Contains(defaultIssuerPermissionGroup))
            {
                permissionGroups.Add(defaultIssuerPermissionGroup);
            }

            // See is the user has already a Issuer, if not create it
            var request = new CreateOrUpdateIssuerRequest()
            {
                LoginName = wsUser,
                Email = wsUser,
                Name = wsUser,
                Password = _WebSealIssuerPassword,
                PermissionGroupNames = permissionGroups,
            };

            AuthenticationHelpers.SetupServiceRequest(request, "CreateIssuer");
            var response = ServiceCallFactory.IssuerService_CreateOrUpdateIssuer(request);

            // Log in issuer (Boxplosive platform)
            // Note that 'hashedPassword' Represents the user name
            PasswordVerificationResult passwordResult = httpContext.GetOwinContext().GetUserManager<ApplicationUserManager>()
                .PasswordHasher.VerifyHashedPassword(wsUser, _WebSealIssuerPassword);

            if (passwordResult != PasswordVerificationResult.Success)
            {
                if (_Logger.IsDebugEnabled)
                    _Logger.Debug("webseal unauthorized: invalid issuer password");

                return false;
            }

            // Create claims identity
            Task<ClaimsIdentity> identityResult = httpContext.GetOwinContext().GetUserManager<ApplicationUserManager>()
                .CreateIdentityAsync(new ApplicationUser(0, wsUser), "ApplicationCookie");

            // Add Boxplosive Roles/Permissions as claim roles (ugly but it works)
            foreach (var role in response.Account.Permissions)
            {
                identityResult.Result.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Set new ClaimsPrincipal
            HttpContext.Current.User = new ClaimsPrincipal(identityResult.Result);

            // Create ApplicationCookie
            httpContext.GetOwinContext().Get<ApplicationSignInManager>().AuthenticationManager
                .SignIn(new AuthenticationProperties() { IsPersistent = true }, identityResult.Result);

            if (_Logger.IsDebugEnabled)
                _Logger.Debug("webseal authorized");

            return true;
        }

        private bool WebSealHostIsValid(string requestHostName, string requestIpAddress, HashSet<string> wsHostWhitelist)
        {
            if (_Logger.IsTraceEnabled)
            {
                _Logger.Trace($"request hostname={requestHostName}");
                _Logger.Trace($"request ip address={requestIpAddress}");
            }

            return (!string.IsNullOrWhiteSpace(requestHostName) && wsHostWhitelist.Contains(requestHostName))
                || (!string.IsNullOrWhiteSpace(requestIpAddress) && wsHostWhitelist.Contains(requestIpAddress));
        }

        private string GetWebSealUser()
        {
            string headerKey = AppConfig.Settings.WebSealUserHeader;
            string headerValue = null;
            if (!string.IsNullOrWhiteSpace(headerKey))
            {
                headerValue = HttpContext.Current.Request.Headers.Get(headerKey);
            }

            if (_Logger.IsDebugEnabled)
                _Logger.Debug($"webseal header {headerKey}={headerValue}");

            return headerValue;
        }

        /// <remarks>
        /// The iv-groups header consists of comma separated quoted entries.
        /// </remarks>
        private IList<string> GetWebSealGroups()
        {
            string headerKey = AppConfig.Settings.WebSealGroupsHeader;
            string headerValue = null;
            if (!string.IsNullOrWhiteSpace(headerKey))
            {
                headerValue = HttpContext.Current.Request.Headers.Get(headerKey);
            }

            if (_Logger.IsDebugEnabled)
                _Logger.Debug($"webseal header {headerKey}={headerValue}");

            var result = new List<string>();
            if (headerValue != null)
            {
                result = _WebSealGroupsRegex
                    .Split(headerValue)
                    .Select(item => item.Trim().Trim('"'))
                    .Where(item => !string.IsNullOrWhiteSpace(item))
                    .ToList();
            }

            if (_Logger.IsDebugEnabled)
                _Logger.Debug($"WebSealGroups={string.Join(";", result)}");

            return result;
        }

        private IList<string> GetPermissionGroups(ICollection<string> wsGroups)
        {
            var result = new List<string>();
            foreach (string wsGroup in wsGroups)
            {
                if (WebSealToPermissionGroupsMapping.TryGetValue(wsGroup, out string permissionGroup))
                {
                    result.Add(permissionGroup);
                }
                else if (AppConfig.Settings.WebSealGroupExpectedPrefix != null && wsGroup.StartsWith(AppConfig.Settings.WebSealGroupExpectedPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    if (_Logger.IsWarnEnabled)
                        _Logger.Warn($"webseal group={wsGroup} not recognized even though it starts with prefix={AppConfig.Settings.WebSealGroupExpectedPrefix}");
                }
            }

            if (_Logger.IsDebugEnabled)
                _Logger.Debug($"PermissionGroups={string.Join(";", result)}");

            return result;
        }

        private static HashSet<string> WebSealHostWhitelist
        {
            get
            {
                string wsHostWhitelistValue = AppConfig.Settings.WebSealHostWhitelist;
                if (_WebSealHostWhitelistConfigValue != wsHostWhitelistValue || _WebSealHostWhitelist == null)
                {
                    _WebSealHostWhitelistConfigValue = wsHostWhitelistValue;

                    wsHostWhitelistValue = wsHostWhitelistValue ?? string.Empty;
                    _WebSealHostWhitelist = new HashSet<string>(wsHostWhitelistValue
                        .Split(new char[] { ';', ',' })
                        .Select(item => item.Trim())
                        .Where(item => !string.IsNullOrWhiteSpace(item))
                    );

                    if (_Logger.IsDebugEnabled)
                        _Logger.Debug($"WebSealHostWhitelist={string.Join(";", _WebSealHostWhitelist)}");
                }

                return _WebSealHostWhitelist;
            }
        }

        private static HashSet<string> _WebSealHostWhitelist;
        private static string _WebSealHostWhitelistConfigValue;

        /// <remarks>
        /// key=[WebSEAL group name]
        /// value=[permission group name]
        /// </remarks>
        private static IDictionary<string, string> WebSealToPermissionGroupsMapping
        {
            get
            {
                string wsToPermissionGroupsMappingValue = AppConfig.Settings.WebSealToPermissionGroupsMapping;
                if (_WebSealToPermissionGroupsMappingConfigValue != wsToPermissionGroupsMappingValue || _WebSealToPermissionGroupsMapping == null)
                {
                    _WebSealToPermissionGroupsMappingConfigValue = wsToPermissionGroupsMappingValue;

                    wsToPermissionGroupsMappingValue = wsToPermissionGroupsMappingValue ?? string.Empty;
                    IList<string> wsToPermissionGroupsMapping = wsToPermissionGroupsMappingValue
                        .Split(new char[] { ';', ',' })
                        .Select(item => item.Trim())
                        .Where(item => !string.IsNullOrWhiteSpace(item))
                        .ToList();

                    _WebSealToPermissionGroupsMapping = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (string wsToPermissionGroupItem in wsToPermissionGroupsMapping)
                    {
                        IList<string> wsToPermissionGroupParts = wsToPermissionGroupItem
                            .Split(new char[] { '|' })
                            .Select(item => item.Trim())
                            .Where(item => !string.IsNullOrWhiteSpace(item))
                            .ToList();
                        if (wsToPermissionGroupParts.Count != 2)
                        {
                            continue; // invalid
                        }

                        _WebSealToPermissionGroupsMapping.Add(wsToPermissionGroupParts[0], wsToPermissionGroupParts[1]);
                    }

                    if (_Logger.IsDebugEnabled)
                        _Logger.Debug($"WebSealToPermissionGroupsMapping={string.Join(";", _WebSealToPermissionGroupsMapping.Select(e => $"{e.Key}/{e.Value}"))}");
                }

                return _WebSealToPermissionGroupsMapping;
            }
        }

        /// <summary>
        /// Copy from System.Web.Mvc.AuthorizeAttribute's.
        /// </summary>
        internal static string[] SplitString(string original)
        {
            if (string.IsNullOrEmpty(original))
                return new string[0];

            return ((IEnumerable<string>)original.Split(',')).Select(piece => new
            {
                piece = piece,
                trimmed = piece.Trim()
            }).Where(_param0 => !string.IsNullOrEmpty(_param0.trimmed)).Select(_param0 => _param0.trimmed).ToArray<string>();
        }

        private static IDictionary<string, string> _WebSealToPermissionGroupsMapping;
        private static string _WebSealToPermissionGroupsMappingConfigValue;

        private const string _WebSealIssuerPassword = "b!SAbu_Asw8$tEqABRu4";
        private static readonly Regex _WebSealGroupsRegex = new Regex("(?<=^[^\"]*(?:\"[^\"]*\"[^\"]*)*),(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)", RegexOptions.Compiled);

        #endregion Helpers
    }
}