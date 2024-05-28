using nl.boxplosive.BackOffice.Mvc.Html;
using nl.boxplosive.Data.Sdk;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class IssuerEditModel : IssuerModelBase
    {
        /// <remarks>
        /// Optional field.
        /// </remarks>
        [Display(Name = "New password")]
        public override string Password { get; set; }

        /// <remarks>
        /// ELP-8052: Is used for disabling edit/delete operation for this account.
        ///   If is admin, currently the edit/delete button are displayed on the delete page.
        ///   It would be nice if the buttons are not displayed, but that is not the case for now because
        ///     then all accounts and all permissions groups are lazy loaded on the DTO, which reluts in alot of database calls.
        /// </remarks>
        public bool IsAdmin { get; private set; }

        public IssuerEditModel()
            : base()
        {
        }

        public IssuerEditModel(int id)
            : this()
        {
            IDtoIssuer issuer = IssuerRepository.Get(id);

            Id = issuer.Id;
            Version = issuer.Version;
            LoginName = issuer.Username;
            Email = issuer.Email;
            Name = issuer.Name;
            CheckPermissionGroupBoxes(issuer.Account.PermissionGroups);

            // ELP-8052: Not allowed to edit/delete admin issuers (PermissionGroup=AdminIssuer)
            //   This kind of issuers are not managed from within the backoffice
            IsAdmin = issuer.Account.PermissionGroups.Any(x => x.Name.Equals("AdminIssuer", StringComparison.OrdinalIgnoreCase));
        }

        private void CheckPermissionGroupBoxes(IList<IDtoPermissionGroup> permissionGroups)
        {
            var permissionGroupNames = new HashSet<string>(permissionGroups.Select(x => x.Name));
            foreach (CheckBoxListItem checkBoxListItem in PermissionGroups)
            {
                if (permissionGroupNames.Contains(checkBoxListItem.Value))
                    checkBoxListItem.IsChecked = true;
            }
        }
    }
}