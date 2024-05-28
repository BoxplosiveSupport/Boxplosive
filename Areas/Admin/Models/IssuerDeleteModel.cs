using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.MsSql;
using nl.boxplosive.Data.Sdk;
using System;
using System.Linq;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class IssuerDeleteModel : VersionedViewModelBase
    {
        public string LoginName { get; set; }

        /// <remarks>
        /// ELP-8052: Is used for disabling edit/delete operation for this account.
        ///   If is admin, currently the edit/delete button are displayed on the delete page.
        ///   It would be nice if the buttons are not displayed, but that is not the case for now because
        ///     then all accounts and all permissions groups are lazy loaded on the DTO, which reluts in alot of database calls.
        /// </remarks>
        public bool IsAdmin { get; private set; }

        public IssuerDeleteModel()
            : base()
        {

        }

        public IssuerDeleteModel(int id)
            : this()
        {
            IDtoIssuer issuer = IssuerRepository.Get(id);

            Id = issuer.Id;
            Version = issuer.Version;
            LoginName = issuer.Username;

            // ELP-8052: Not allowed to edit/delete admin issuers (PermissionGroup=AdminIssuer)
            //   This kind of issuers are not managed from within the backoffice
            IsAdmin = issuer.Account.PermissionGroups.Any(x => x.Name.Equals("AdminIssuer", StringComparison.OrdinalIgnoreCase));
        }

        public void Delete()
        {
            IDtoIssuer issuer = IssuerRepository.Get(Id);
            // ELP-8052: Not allowed to edit/delete admin issuers (PermissionGroup=AdminIssuer)
            //   This kind of issuers are not managed from within the backoffice
            if (issuer.Account.PermissionGroups.Any(x => x.Name.Equals("AdminIssuer", StringComparison.OrdinalIgnoreCase)))
                return;

            // Temp store account before issuer will be deleted
            IDtoAccount account = issuer.Account;

            IssuerRepository.Delete(Id, Version);

            // Disable account
            account.IsApproved = false;
            account.IsEnabled = false;
            AccountRepository.Update(account);

            // Delete all active sessions
            foreach (IDtoSession session in SessionRepository.GetByAccount(account))
                SessionRepository.Delete(session);
        }
    }
}