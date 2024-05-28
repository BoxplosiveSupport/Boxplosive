using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Business.Sdk.Exceptions;
using nl.boxplosive.Configuration;
using nl.boxplosive.Service.ServiceContract.AuthenticationService.DataContracts;
using nl.boxplosive.Service.ServiceModel.Authentication;
using nl.boxplosive.Utilities.Conversion;
using nl.boxplosive.Utilities.Extensions;

namespace nl.boxplosive.BackOffice.Mvc
{
    public class ApplicationUserStore :
        IUserStore<ApplicationUser, int>,
        IUserLockoutStore<ApplicationUser, int>,
        IUserPasswordStore<ApplicationUser, int>,
        IUserTwoFactorStore<ApplicationUser, int>
    {
        #region IUserStore

        public Task CreateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<ApplicationUser> FindByIdAsync(int userId)
        {
            return Task.FromResult(new ApplicationUser(userId, "Unknown"));
        }

        public Task<ApplicationUser> FindByNameAsync(string userName)
        {
            return Task.FromResult(new ApplicationUser(0, userName));
        }

        public Task UpdateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        #endregion IUserStore

        #region IUserLockoutStore

        public Task<int> GetAccessFailedCountAsync(ApplicationUser user)
        {
            return Task.FromResult(0); //ELP-1683
        }

        public Task<bool> GetLockoutEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        public Task<DateTimeOffset> GetLockoutEndDateAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task<int> IncrementAccessFailedCountAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task ResetAccessFailedCountAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEnabledAsync(ApplicationUser user, bool enabled)
        {
            throw new NotImplementedException();
        }

        public Task SetLockoutEndDateAsync(ApplicationUser user, DateTimeOffset lockoutEnd)
        {
            throw new NotImplementedException();
        }

        #endregion IUserLockoutStore

        #region IUserPasswordStore

        public Task<string> GetPasswordHashAsync(ApplicationUser user)
        {
            return Task.FromResult(user.UserName.ToString());
        }

        public Task<bool> HasPasswordAsync(ApplicationUser user)
        {
            throw new NotImplementedException();
        }

        public Task SetPasswordHashAsync(ApplicationUser user, string passwordHash)
        {
            throw new NotImplementedException();
        }

        #endregion IUserPasswordStore

        #region IUserTwoFactorStore

        public Task<bool> GetTwoFactorEnabledAsync(ApplicationUser user)
        {
            return Task.FromResult(false);
        }

        public Task SetTwoFactorEnabledAsync(ApplicationUser user, bool enabled)
        {
            throw new NotImplementedException();
        }

        #endregion IUserTwoFactorStore

        public void Dispose()
        {
            // TODO
        }
    }

    // Configure the application user manager used in this application. UserManager is defined in ASP.NET Identity and is used by the application.
    public class ApplicationUserManager : UserManager<ApplicationUser, int>
    {
        public ApplicationUserManager(IUserStore<ApplicationUser, int> store)
            : base(store)
        {
            this.PasswordHasher = new ApplicationPasswordHasher();
        }

        public static ApplicationUserManager Create(IdentityFactoryOptions<ApplicationUserManager> options, IOwinContext context)
        {
            var manager = new ApplicationUserManager(new ApplicationUserStore());

            return manager;
        }

        public override Task<ClaimsIdentity> CreateIdentityAsync(ApplicationUser user, string authenticationType)
        {
            // Retrieve session ticket result of login
            string sessionTicket = (string)HttpContext.Current.Items["UserData"];

            // Remove temporary stored session ticket from current context
            HttpContext.Current.Items.Remove("UserData");

            // Create 'claims' identity
            var claims = new List<Claim>() {
                new Claim(ClaimTypes.PrimarySid, sessionTicket),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Expiration, DateTime.UtcNow.Add(TimeSpan.FromSeconds(AppConfig.Settings.LoginExpirationTimeout)).ToDatabaseStringByKind())
            };
            var claimsIdentity = new ClaimsIdentity(claims, authenticationType);

            return Task.FromResult(claimsIdentity);
        }
    }

    // Configure the application sign-in manager which is used in this application.
    public class ApplicationSignInManager : SignInManager<ApplicationUser, int>
    {
        public ApplicationSignInManager(ApplicationUserManager userManager, IAuthenticationManager authenticationManager)
            : base(userManager, authenticationManager)
        {
        }

        public override Task<ClaimsIdentity> CreateUserIdentityAsync(ApplicationUser user)
        {
            return user.GenerateUserIdentityAsync((ApplicationUserManager)UserManager);
        }

        public static ApplicationSignInManager Create(IdentityFactoryOptions<ApplicationSignInManager> options, IOwinContext context)
        {
            return new ApplicationSignInManager(context.GetUserManager<ApplicationUserManager>(), context.Authentication);
        }
    }

    #region Helper classes

    /// <summary>
    /// Use Custom approach to verify password
    /// </summary>
    public class ApplicationPasswordHasher : PasswordHasher
    {
        public ApplicationPasswordHasher()
        {
        }

        public override string HashPassword(string password)
        {
            throw new NotImplementedException();
        }

        public override PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            // Create login request
            var loginRequest = new LoginRequest
            {
                UserType = UserTypes.BackOffice,
                UserName = hashedPassword, // HACK: Represents the user name
                Password = providedPassword,
                ExpirationTimeout = AppConfig.Settings.LoginExpirationTimeout
            };
            AuthenticationHelpers.SetupServiceRequest(loginRequest, "Login");

            // TODO Remove try-catch if service handles exceptions properly
            LoginResponse result = null;
            try
            {
                result = ServiceCallFactory.Authentication_Login(loginRequest);
            }
            catch (InvalidCredentialsException)
            {
                // TODO log
            }

            if (result == null || !result.Success)
            {
                return PasswordVerificationResult.Failed;
            }

            // Temporary store session ticket in current context
            HttpContext.Current.Items["UserData"] = result.Session.SessionTicket;
            HttpContext.Current.Items["BoxplosivePermissions"] = result.Account.Permissions;

            return PasswordVerificationResult.Success;
        }
    }

    #endregion Helper classes
}