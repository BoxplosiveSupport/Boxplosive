using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.BackOffice.Mvc.Html;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.BackOffice.Mvc.Utils;
using nl.boxplosive.Service.ServiceContract.IssuerService.DataContracts;
using System.Linq;
using System.Web.Http.ModelBinding;
using System.Web.UI.WebControls;
using System.Globalization;
using System.Web.Mvc;
using Newtonsoft.Json.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public abstract class IssuerModelBase : VersionedViewModelBase
    {
        [Display(Name = "Login name")]
        [Required]
        public string LoginName { get; set; }

        public readonly bool LoginName_DisplayField;

        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        public virtual string Password { get; set; }

        public readonly bool Password_DisplayField;

        [Display(Name = "Roles")]
        public IList<CheckBoxListItem> PermissionGroups { get; set; }

        public IssuerModelBase()
            : base()
        {
            bool b2cEnabled = Globals.B2C_Enabled;

            LoginName_DisplayField = !b2cEnabled;
            Password_DisplayField = !b2cEnabled;
            PermissionGroups = GetPermissionGroups();

            // RequiredIfAttribute breaks client-side validation for some reason, this is a workarrround
            if (!LoginName_DisplayField)
                LoginName = "value";

            if (!Password_DisplayField)
                Password = "value";
        }

        public void CreateOrEdit(bool isCreate)
        {
            CreateOrUpdateIssuerRequest request = ToCreateOrUpdateIssuerRequest(isCreate);
            AuthenticationHelpers.SetupServiceRequest(request, "CreateOrUpdateIssuer");
            CreateOrUpdateIssuerResponse createIssuerResponse = ServiceCallFactory.IssuerService_CreateOrUpdateIssuer(request);
        }

        public void TrimFields(System.Web.Mvc.ModelStateDictionary modelState, bool isCreate)
        {
            // ELP-8088: trim fields, keep LoginName/Email as is on update because it can't be changed
            if (isCreate)
            {
                LoginName = LoginName?.Trim();
                // Must set value to ModelState otherwise this change not applied when displaying View
                SetValueToModelState(modelState, nameof(LoginName), LoginName);
            }

            // If B2C is disabled the email can changed on update
            if (isCreate || !Globals.B2C_Enabled)
            {
                Email = Email?.Trim();
                SetValueToModelState(modelState, nameof(Email), Email);
            }

            Name = Name?.Trim();
            SetValueToModelState(modelState, nameof(Name), Name);
        }

        public void SetIssuerAlreadyExistsModelError(System.Web.Mvc.ModelStateDictionary modelState)
        {
            if (LoginName_DisplayField)
                modelState.AddModelError(nameof(LoginName), "The Login name is already in use.");
            else
                modelState.AddModelError(nameof(Email), "The Email is already in use.");
        }

        private CreateOrUpdateIssuerRequest ToCreateOrUpdateIssuerRequest(bool isCreate)
        {
            if (Globals.B2C_Enabled)
            {
                LoginName = Email;
                // Note that when password is not provided, on update password is not changed
                Password = isCreate ? Globals.DefaultIssuerPassword_AzAdB2c_User : string.Empty;
            }

            return new CreateOrUpdateIssuerRequest()
            {
                LoginName = LoginName,
                Email = Email,
                Name = Name,
                Password = Password,
                PermissionGroupNames = PermissionGroups.Where(x => x.IsChecked).Select(x => x.Value).ToList(),
                IsExplicitCreate = isCreate,
            };
        }

        private IList<CheckBoxListItem> GetPermissionGroups()
        {
            return new List<CheckBoxListItem>()
            {
                new CheckBoxListItem() { Text = "Application Manager", Value = "ApplicationManager" },
                new CheckBoxListItem() { Text = "Campaign Employee", Value = "CampaignEmployee" },
                new CheckBoxListItem() { Text = "Campaign Manager", Value = "CampaignManager" },
                new CheckBoxListItem() { Text = "Data-entry Employee", Value = "DataEntryEmployee" },
                new CheckBoxListItem() { Text = "Data-entry Manager", Value = "DataEntryManager" },
                new CheckBoxListItem() { Text = "Reclamation Employee", Value = "ReclamationEmployee" },
                new CheckBoxListItem() { Text = "Reclamation Manager", Value = "ReclamationManager" },
                new CheckBoxListItem() { Text = "Reclamation Reader", Value = "ReclamationReader" },
            };
        }

        /// <remarks>
        /// https://stackoverflow.com/a/2755815
        /// </remarks>
        private void SetValueToModelState(System.Web.Mvc.ModelStateDictionary modelState, string key, object value)
        {
            modelState.SetModelValue(key, new ValueProviderResult(value, value?.ToString(), CultureInfo.InvariantCulture));
        }
    }
}