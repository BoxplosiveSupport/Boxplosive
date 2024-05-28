using System.Collections.Generic;
using System.Linq;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Shared;
using nl.boxplosive.Business.Sdk.Entities;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Models.LoyaltyEventDefinition
{
    public class LoyaltyEventDefinitionsModel : ViewModelBase
    {
        public LoyaltyEventDefinitionsModel()
        {
        }

        public LoyaltyEventDefinitionsModel(LoyaltyEventDefinitions loyaltyEventDefinitions)
        {
            LoyaltyEventDefinitions = loyaltyEventDefinitions.Select(l => new LoyaltyEventDefinitionModel(l)).ToList();

            PagedList = new StaticPagedList<LoyaltyEventDefinitionModel>(
                LoyaltyEventDefinitions, loyaltyEventDefinitions.PageNumber, loyaltyEventDefinitions.PageSize, loyaltyEventDefinitions.TotalRecordCount);
        }

        public IList<LoyaltyEventDefinitionModel> LoyaltyEventDefinitions { get; }

        public IPagedList<LoyaltyEventDefinitionModel> PagedList { get; set; }

        public Modal ConfirmDeleteModal = GetConfirmDeleteModal(LoyaltyEventDefinitionFactory.PageTitle);
    }
}