using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Service.ServiceModel.Customer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class ReclamationModel : ViewModelBase
    {
        public ReclamationModel()
        {
            CouponTab = new CouponTabModel();
            LoyaltyTab = new LoyaltyTabModel();
        }

        [Required]
        public string SearchTerm { get; set; }

        public CustomerDetailsType SearchType { get; set; } = CustomerDetailsType.LoyaltyCardNumber;

        public IList<SelectListItem> SearchTypes
        {
            get
            {
                if (_SearchTypes == null)
                {
                    IList<DtoCustomerProfileField> customerProfileFieldDefinitions = CustomerProfileRepository.GetAllFields();

                    var searchTypeSelectListItems = EnumHelper.GetSelectList(typeof(CustomerDetailsType)).ToList();
                    RemoveItemFromSearchTypesSelectListIfCustomerProfileFieldDefinitionNotExists(DataConfig.Instance.CustomerProfileExternalIdKey,
                        CustomerDetailsType.CustomerExternalId, searchTypeSelectListItems, customerProfileFieldDefinitions);
                    RemoveItemFromSearchTypesSelectListIfCustomerProfileFieldDefinitionNotExists(DataConfig.Instance.CustomerProfileUsernameKey,
                        CustomerDetailsType.Username, searchTypeSelectListItems, customerProfileFieldDefinitions);

                    _SearchTypes = searchTypeSelectListItems;
                }

                return _SearchTypes;
            }
        }

        private IList<SelectListItem> _SearchTypes { get; set; }

        [DisplayName("Customer ID")]
        public int CustomerId { get; set; }

        [DisplayName("Customer external ID")]
        public string CustomerExternalId { get; set; }

        [DisplayName("Digital loyalty card number")]
        public string LoyaltyCardNumber_Decrypted_MainNonOptional { get; set; }

        [DisplayName("Physical loyalty card number")]
        public string LoyaltyCardNumber_Decrypted_MainOptional { get; set; }

        [DisplayName("Email address")]
        public string Username_Decrypted { get; set; }

        public CouponTabModel CouponTab { get; set; }

        public LoyaltyTabModel LoyaltyTab { get; set; }

        public string SelectedTab { get; set; }

        public Modal ConfirmDeleteLoyaltyCardMainOptionalModal = GetConfirmDeleteModal("Physical loyalty card");

        public bool ShowData
        {
            get
            {
                return CustomerId > 0 &&
                       CouponTab != null &&
                       LoyaltyTab != null;
            }
        }

        public bool SearchTypes_Contains_CustomerExternalId()
        {
            return SearchTypes.Any(item => int.Parse(item.Value) == (int)CustomerDetailsType.CustomerExternalId);
        }

        public bool SearchTypes_Contains_Username()
        {
            return SearchTypes.Any(item => int.Parse(item.Value) == (int)CustomerDetailsType.Username);
        }

        private void RemoveItemFromSearchTypesSelectListIfCustomerProfileFieldDefinitionNotExists(string customerProfileFieldDefinition,
            CustomerDetailsType searchTypeSelectListItemValue, List<SelectListItem> searchTypeSelectListItems,
            IList<DtoCustomerProfileField> customerProfileFieldDefinitions)
        {
            if (!customerProfileFieldDefinitions.Any(item => item.Key.Equals(customerProfileFieldDefinition, StringComparison.OrdinalIgnoreCase)))
            {
                int index = searchTypeSelectListItems.FindIndex(item => int.Parse(item.Value) == (int)searchTypeSelectListItemValue);
                if (index != -1)
                    searchTypeSelectListItems.RemoveAt(index);
            }
        }
    }
}