using Microsoft.AspNet.Identity;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin;
using Microsoft.Owin.Host.SystemWeb;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Notifications;
using Microsoft.Owin.Security.OpenIdConnect;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Utils;
using nl.boxplosive.Business.Sdk;
using nl.boxplosive.Business.Sdk.Apis;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceContract.AuthenticationService.DataContracts;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Utilities.Json;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace nl.boxplosive.BackOffice.Mvc
{
    public partial class Startup
    {
        private static readonly NLog.Logger _Logger = NLog.LogManager.GetCurrentClassLogger();

        /// <remarks>
        /// Obsolete: this can be removed when the BackOffice.Mvc is eliminated
        /// </remarks>
        private readonly PathString _LoginPath = new PathString("/Account/Login");

        /// <summary>
        /// Configure the OWIN middleware
        /// </summary>
        /// <remarks>
        /// For more information on configuring authentication, please visit http://go.microsoft.com/fwlink/?LinkId=301864
        /// </remarks>
        public void ConfigureAuth(IAppBuilder app)
        {
            if (Globals.B2C_Enabled)
            {
                app.SetDefaultSignInAsAuthenticationType(CookieAuthenticationDefaults.AuthenticationType);

                app.UseCookieAuthentication(new CookieAuthenticationOptions
                {
                    // ASP.NET web host compatible cookie manager
                    CookieManager = new SystemWebChunkingCookieManager()
                });

                app.UseOpenIdConnectAuthentication(
                    new OpenIdConnectAuthenticationOptions
                    {
                        // Generate the metadata address using the tenant and policy information
                        MetadataAddress = Globals.B2C_WellKnownMetadata,

                        // These are standard OpenID Connect parameters, with values pulled from web.config
                        ClientId = Globals.B2C_BO_App_Id,
                        RedirectUri = Globals.B2C_BO_RedirectUri,
                        PostLogoutRedirectUri = Globals.B2C_BO_PostLogoutRedirectUri,

                        // Specify the callbacks for each type of notifications
                        Notifications = new OpenIdConnectAuthenticationNotifications
                        {
                            AuthenticationFailed = OnAuthenticationFailed,
                            AuthorizationCodeReceived = OnAuthorizationCodeReceived,
                            MessageReceived = OnMessageReceived,
                            SecurityTokenReceived = OnSecurityTokenReceived,
                            SecurityTokenValidated = OnSecurityTokenValidated,
                            RedirectToIdentityProvider = OnRedirectToIdentityProvider,
                            TokenResponseReceived = OnTokenResponseReceived,
                        },

                        // Specify the claim type that specifies the Name property.
                        TokenValidationParameters = new TokenValidationParameters
                        {
                            NameClaimType = "name",
                            ValidAudience = Globals.B2C_BO_App_Id,
                            ValidIssuer = Globals.B2C_TokenIssuer,
                        },

                        // Specify the scope by appending all of the scopes requested into one string (separated by a blank space)
                        Scope = $"openid",

                        // ASP.NET web host compatible cookie manager
                        CookieManager = new SystemWebCookieManager()
                    }
                );

                return;
            }

            // Configure the user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable the application to use a cookie to store information for the signed in user
            // and to use a cookie to temporarily store information about a user logging in with a third party login provider
            // Configure the sign in cookie
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                CookieSecure = CookieSecureOption.Always,
                LoginPath = _LoginPath,
                ExpireTimeSpan = TimeSpan.FromSeconds(AppConfig.Settings.LoginSlidingExpirationTimeout),
                SlidingExpiration = true,
            });
        }

        /// <summary>
        /// Invoked if exceptions are thrown during request processing. The exceptions will be re-thrown after this event unless suppressed.
        /// </summary>
        /// <remarks>
        /// Catch any failures received by the authentication middleware and handle appropriately
        /// </remarks>
        private Task OnAuthenticationFailed(AuthenticationFailedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            notification.HandleResponse();

            if (notification?.Exception != null)
                _Logger.Error(notification.Exception);

            // Handle custom exceptions thrown when Boxplosive Issuer validation fails for Azure B2c user
            if (notification.Exception.Message.StartsWith("OnSecurityTokenValidated:", StringComparison.OrdinalIgnoreCase))
            {
                // Note that when using the notification.Response.Redirect() mechanism this causes endless loops
                // Note that when setting HttpStatusCode.Unauthorized (401) this cause endless loop
                // @todo ELP-8050: Set status code HttpStatusCode.Unauthorized (401) and fix endless loop
                // @todo ELP-8050: Show a nice error page instead of a default IIS one (this is a general backoffice issue)
                notification.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            // https://github.com/Azure-Samples/active-directory-b2c-dotnet-webapp-and-webapi/blob/main/TaskWebApp/App_Start/Startup.Auth.cs
            // --> line 103
            else if (notification.Exception.Message == "access_denied")
            {
                notification.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
            else
            {
                notification.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked after security token validation if an authorization code is present in the protocol message.
        /// </summary>
        /// <remarks>
        /// Callback function when an authorization code is received
        /// </remarks>
        private async Task OnAuthorizationCodeReceived(AuthorizationCodeReceivedNotification notification)
        {
            try
            {
                /*
				 The `MSALPerUserMemoryTokenCache` is created and hooked in the `UserTokenCache` used by `IConfidentialClientApplication`.
				 At this point, if you inspect `ClaimsPrinciple.Current` you will notice that the Identity is still unauthenticated and it has no claims,
				 but `MSALPerUserMemoryTokenCache` needs the claims to work properly. Because of this sync problem, we are using the constructor that
				 receives `ClaimsPrincipal` as argument and we are getting the claims from the object `AuthorizationCodeReceivedNotification context`.
				 This object contains the property `AuthenticationTicket.Identity`, which is a `ClaimsIdentity`, created from the token received from
				 Azure AD and has a full set of claims.
				 */
                IConfidentialClientApplication confidentialClient = MsalAppBuilder.BuildConfidentialClientApplication(new ClaimsPrincipal(notification.AuthenticationTicket.Identity));

                // Upon successful sign in, get & cache a token using MSAL
                AuthenticationResult result = await confidentialClient.AcquireTokenByAuthorizationCode(Globals.Api_Scopes, notification.Code).ExecuteAsync();
            }
            catch (Exception ex)
            {
                throw new HttpResponseException(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    ReasonPhrase = $"Unable to get authorization code {ex.Message}."
                });
            }
        }

        /// <summary>
        /// Invoked when a protocol message is first received.
        /// </summary>
        private Task OnMessageReceived(MessageReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked with the security token that has been extracted from the protocol message.
        /// </summary>
        private Task OnSecurityTokenReceived(SecurityTokenReceivedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked after the security token has passed validation and a ClaimsIdentity has been generated. Note there are additional checks after this
        /// event that validate other aspects of the authentication flow like the nonce.
        /// </summary>
        private Task OnSecurityTokenValidated(SecurityTokenValidatedNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            var claimsIdentity = notification.AuthenticationTicket.Identity;

            // Extract the email claim
            string emailClaimValue = claimsIdentity.FindFirstValue("email")?.Trim();
            if (emailClaimValue == null)
            {
                string emailsClaimValue = claimsIdentity.FindFirstValue("emails")?.Trim();
                if (emailsClaimValue != null)
                {
                    // Note that emails claim can contain either 'email@somedomain.com' or ['email@somedomain.com']
                    if (emailsClaimValue.StartsWith("[", StringComparison.OrdinalIgnoreCase) && emailsClaimValue.EndsWith("]", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            var emails = JsonWrapper.DeserializeObject<List<string>>(emailsClaimValue);
                            emailClaimValue = emails?.FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            _Logger.Warn(ex, $"OnSecurityTokenValidated: Failed to deserialize \"emails\" Claim, expected a JSON array, actually is \"{emailsClaimValue}\"");
                        }
                    }
                    else
                    {
                        emailClaimValue = emailsClaimValue;
                    }
                }
            }

            if (emailClaimValue == null)
            {
                throw new Exception("OnSecurityTokenValidated: Failed to retrieve \"email\" or \"emails\" Claim from Claims");
            }

            // Retrieve the issuer by email
            var issuerApi = BusinessApiFactory.GetInstance().BusinessApi<IIssuerApi>();
            Issuer issuer = issuerApi.GetByLoginName(emailClaimValue);

            if (issuer == null)
            {
                throw new Exception($"OnSecurityTokenValidated: Failed to retrieve issuer by loginName \"{emailClaimValue}\"");
            }

            // Extract the expiration time claim
            string expClaimValue = claimsIdentity.FindFirstValue("exp");
            if (expClaimValue == null)
            {
                expClaimValue = claimsIdentity.FindFirstValue(ClaimTypes.Expiration);
            }

            TimeSpan loginRequest_ExpirationTimeout;
            if (expClaimValue != null)
            {
                loginRequest_ExpirationTimeout = DateTimeOffset.FromUnixTimeSeconds(long.Parse(expClaimValue)).Subtract(DateTimeOffset.UtcNow);
            }
            else
            {
                loginRequest_ExpirationTimeout = TimeSpan.FromSeconds(AppConfig.Settings.LoginExpirationTimeout);
            }

            // Login issuer
            var loginRequest = new LoginRequest
            {
                UserType = UserTypes.BackOffice,
                UserName = issuer.Username,
                Password = Globals.DefaultIssuerPassword_AzAdB2c_User,
                ExpirationTimeout = (int)loginRequest_ExpirationTimeout.TotalSeconds,
            };
            AuthenticationHelpers.SetupServiceRequest(loginRequest, "Login");

            var loginIssuerResponse = ServiceCallFactory.Authentication_Login(loginRequest);

            // Add session ticket as claim
            string sessionTicket = loginIssuerResponse.Session.SessionTicket;

            claimsIdentity.AddClaim(new Claim(ClaimTypes.PrimarySid, sessionTicket));

            // Add "http://schemas.microsoft.com/ws/2008/06/identity/claims/role" claim, for each account permission
            foreach (var role in loginIssuerResponse.Account.Permissions)
            {
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Add "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name" claim, this is required to have the username displayed in the backoffice
            if (claimsIdentity.FindFirstValue(ClaimTypes.Name) == null)
            {
                string nameClaimValue = claimsIdentity.FindFirstValue("name") ?? "Issuer X";
                claimsIdentity.AddClaim(new Claim(ClaimTypes.Name, nameClaimValue));
            }

            // Set claims principal as current HttpContext user
            HttpContext.Current.User = new ClaimsPrincipal(claimsIdentity);

            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked to manipulate redirects to the identity provider for SignIn, SignOut, or Challenge.
        /// </summary>
        /// <remarks>
        /// On each call to Azure AD B2C, check if a policy (e.g. the profile edit or password reset policy) has been specified in the OWIN context.
        /// If so, use that policy when making the call.Also, don't request a code (since it won't be needed).
        /// </remarks>
        private Task OnRedirectToIdentityProvider(RedirectToIdentityProviderNotification<OpenIdConnectMessage, OpenIdConnectAuthenticationOptions> notification)
        {
            var policy = notification.OwinContext.Get<string>("Policy");

            if (!string.IsNullOrEmpty(policy) && !policy.Equals(Globals.B2C_BO_SignInPolicyId))
            {
                notification.ProtocolMessage.Scope = OpenIdConnectScope.OpenId;
                notification.ProtocolMessage.ResponseType = OpenIdConnectResponseType.IdToken;
                notification.ProtocolMessage.IssuerAddress = notification.ProtocolMessage.IssuerAddress.ToLower().Replace(Globals.B2C_BO_SignInPolicyId.ToLower(), policy.ToLower());
            }

            return Task.FromResult(0);
        }

        /// <summary>
        /// Invoked after "authorization code" is redeemed for tokens at the token endpoint.
        /// </summary>
        private Task OnTokenResponseReceived(TokenResponseReceivedNotification notification)
        {
            return Task.FromResult(0);
        }
    }
}