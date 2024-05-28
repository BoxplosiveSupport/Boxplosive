using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    public enum CodeActionTypeModel
    {
        [Display(Name = "One time use code")]
        Code_OneTimeUse = 1,
    }
}