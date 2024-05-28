using System;
using System.Configuration;

namespace nl.boxplosive.BackOffice.Mvc.Utils
{
    public static class Globals
    {
        /// <summary>
        /// E.g. boxplosiveb2ctst
        /// </summary>
        private static string _B2C_Host = ConfigurationManager.AppSettings["B2C_Host"];

        /// <summary>
        /// Azure AD B2C: ClientId
        /// </summary>
        public static string B2C_BO_App_Id = ConfigurationManager.AppSettings["B2C_BO_App_Id"];

        /// <summary>
        /// Azure AD B2C: ClientSecret
        /// </summary>
        public static string B2C_BO_App_Secret = ConfigurationManager.AppSettings["B2C_BO_App_Secret"];

        /// <summary>
        /// Azure AD B2C: TenantId
        /// </summary>
        public static string B2C_HostId = ConfigurationManager.AppSettings["B2C_HostId"];

        /// <summary>
        /// Azure AD B2C: RedirectUri
        /// </summary>
        /// <remarks>
        /// Make sure that URI ends with a forward slash ("/"), otherwise this causes undesired Azure AD B2C redirect behavior
        /// </remarks>
        public static string B2C_BO_RedirectUri
        {
            get
            {
                string uriString = ConfigurationManager.AppSettings["B2C_BO_RedirectUri"];
                
                return !uriString.EndsWith("/") ? $"{uriString}/" : uriString;
            }
        }

        /// <summary>
        /// Azure AD B2C: B2C_BO_PostLogoutRedirectUri
        /// </summary>
        /// <remarks>
        /// Make sure that URI ends with a forward slash ("/"), otherwise this causes undesired Azure AD B2C redirect behavior:
        /// - e.g. it redirect to http://localhost:5288/signin-oidc on boxplosive tst environment
        /// - with response error: The redirect URI &#39;https://boxplosive-tst.boxplosive.net/backoffice/&#39; provided in the request is not registered for the client id &#39;xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx&#39;
        /// </remarks>
        public static string B2C_BO_PostLogoutRedirectUri => new Uri(new Uri(B2C_BO_RedirectUri), "account/loggedout/").AbsoluteUri;

        /// <summary>
        /// Azure AD B2C: SignUpSignInPolicyId
        /// </summary>
        public static string B2C_BO_SignInPolicyId = ConfigurationManager.AppSettings["B2C_BO_SignInPolicyId"];

        /// <summary>
        /// Azure AD B2C: Enabled
        /// </summary>
        public static bool B2C_Enabled = bool.TryParse(ConfigurationManager.AppSettings["B2C_Enabled"], out bool result) ? result : false;

        /// <summary>
        /// Azure AD B2C: B2CAuthority
        /// </summary>
        public static string B2C_Authority = $"https://{_B2C_Host}.b2clogin.com/tfp/{_B2C_Host}.onmicrosoft.com/{B2C_BO_SignInPolicyId}";

        /// <summary>
        /// Azure AD B2C: WellKnownMetadata
        /// </summary>
        public static string B2C_WellKnownMetadata = $"{B2C_Authority}/v2.0/.well-known/openid-configuration";

        /// <summary>
        /// Azure AD B2C: TokenIssuer
        /// </summary>
        public static string B2C_TokenIssuer = $"https://{_B2C_Host}.b2clogin.com/{B2C_HostId}/v2.0/";

        /// <summary>
        /// API Scopes
        /// </summary>
        public static string[] Api_Scopes = new string[0];

        /// <summary>
        /// The default password for an Azure AD B2C issuer.
        /// </summary>
        public static readonly string DefaultIssuerPassword_AzAdB2c_User = "Why_Form_Visitor_Pour_0";
    }
}