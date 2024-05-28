using System.Security.Claims;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceContract;
using nl.boxplosive.Service.ServiceModel.Authentication;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
	public class AuthenticationHelpers
	{
		public static Session GetSession()
		{
			// Note that ClaimTypes.NameIdentifier represents the user's session ticket
			Claim claim = ClaimsPrincipal.Current.FindFirst(c => c.Subject.IsAuthenticated && c.Type == ClaimTypes.PrimarySid);
			if (claim == null)
			{
				return null;
			}

			return new Session()
			{
				SessionTicket = claim.Value
			};
		}

		public static void SetupServiceRequest(ServiceRequestBase request, string methodName)
		{
			request.ApiClientId = AppConfig.Settings.ApiClientId;
			request.Session = GetSession() ?? new Session();
		}
	}
}