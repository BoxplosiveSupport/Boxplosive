using nl.boxplosive.Sdk.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Enums
{
    public enum UserVisibilityModel
    {
        /// <remarks>default</remarks>
        [Display(Name = "All users")]
        AllUsers,

        [Display(Name = "Logged-in users")]
        LoggedInUsers,

        [Display(Name = "Logged-out users")]
        LoggedOutUsers,
    }

    public static class UserVisibilityModelExtensions
    {
        public static string ConvertToString(this UserVisibilityModel value) =>
            Enum.GetName(typeof(UserVisibilityModel), value);            
    
        public static UserVisibilityModel ConvertToUserVisibilityModel(this string value) =>
            (UserVisibilityModel)Enum.Parse(typeof(UserVisibilityModel), value);
    }
}