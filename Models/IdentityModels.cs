using Microsoft.AspNet.Identity;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IUser<int>
    {
        public int Id { get; private set; }

        public string UserName { get; set; }

        /// <summary>
        /// Parameterless constructor.
        /// </summary>
        public ApplicationUser()
        {
        }

        public ApplicationUser(int id, string userName)
        {
            Id = id;
            UserName = userName;
        }

        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser, int> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);

            IEnumerable<string> boxplosivePermissions = HttpContext.Current.Items["BoxplosivePermissions"] as IEnumerable<string>;
            if (boxplosivePermissions != null)
            {
                // Add Boxplosive Roles/Permissions as claim roles (ugly but it works)
                foreach (var role in boxplosivePermissions)
                {
                    userIdentity.AddClaim(new Claim(ClaimTypes.Role, role));
                }
            }

            // Add custom user claims here
            return userIdentity;
        }
    }
}