using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public class LoginViewModel : ViewModelBase
    {
        [Required]
        [Display(Name = "Username")]
        //[EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
