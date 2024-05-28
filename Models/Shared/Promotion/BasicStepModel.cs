using Foolproof;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Service.ServiceModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Principal;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    public class BasicStepModel : StepModelBase
    {
        public BasicStepModel()
        {
        }

        public BasicStepModel(PromotionCampaign serviceModel)
            : this()
        {
            Title = serviceModel.Title;
            CampaignExternalId = serviceModel.CampaignExternalId;
            Description = serviceModel.Description;
            AppCardStackDefinitionId = serviceModel.AppCardStackDefinitionId;
            if (serviceModel.AppCardOrder.HasValue)
                AppCardOrder = serviceModel.AppCardOrder.Value;
            AppCardTemplateId = serviceModel.AppCardTemplateId;
            Placeholders = PlaceholderFactory.GetPlaceholderModels(AppCardTemplateId, serviceModel.PlaceholderValues);
            PromotionActivationRequired = !serviceModel.AutoClaimAllowed;
        }

        public void CopyValuesToServiceModel(PromotionCampaign serviceModel)
        {
            serviceModel.Title = Title;
            serviceModel.CampaignExternalId = CampaignExternalId;
            serviceModel.Description = Description;
            serviceModel.AppCardStackDefinitionId = AppCardStackDefinitionId;
            serviceModel.AppCardOrder = AppCardOrder;
            serviceModel.AppCardTemplateId = AppCardTemplateId;
            serviceModel.PlaceholderValues = Placeholders.Select(item => item.ToServiceModel()).ToList();
            serviceModel.AutoClaimAllowed = !PromotionActivationRequired;
        }

        public bool AppCardStackDefinitionsEnabled { get; } = AppConfig.Settings.Feature_FlexibleAppCardStacks;

        [Required]
        [StringLength(50, ErrorMessage = "Title has a maximum of 50 characters")]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        [ToStringLength(1000, ErrorMessage = "Description has a maximum of 1000 characters")]
        public HtmlEditor Description { get; set; }

        [DisplayName("Campaign external ID")]
        [Required]
        [StringLength(100, ErrorMessage = "Campaign external ID has a maximum of 100 characters")]
        public string CampaignExternalId { get; set; }

        public List<SelectListItem> AppCardStackDefinitions => AppCardStackDefinitionFactory.AppCardStackDefinitionToSelectList(AppCardStackDefinitionId);

        [Display(Name = "AppCard stack")]
        [RequiredIfTrue(nameof(AppCardStackDefinitionsEnabled), ErrorMessage = "The AppCard stack field is required.")]
        public int AppCardStackDefinitionId { get; set; }

        public string AppCardStackDefinitionDisplayName => AppCardStackDefinitionFactory.GetAppCardStackDefinitionDisplayName(AppCardStackDefinitionId);

        [DisplayName("AppCard order")]
        [Required]
        public int AppCardOrder { get; set; } = 100;

        #region AppCardTemplateId

        [DisplayName("AppCard template")]
        [RequiredIfTrue(nameof(AppCardTemplateIdIsDisplayed), ErrorMessage = "The AppCard template field is required.")]
        public int? AppCardTemplateId { get; set; }

        public List<SelectListItem> AppCardTemplateId_SelectListItems
        {
            get
            {
                return TemplateFactory.CreateSelectList(TemplateType.Promotion, AppCardTemplateId);
            }
        }

        public string AppCardTemplateId_Name
        {
            get
            {
                SelectListItem selectedItem = AppCardTemplateId_SelectListItems
                    .FirstOrDefault(item => item.Value == AppCardTemplateId?.ToString());

                return selectedItem?.Text ?? AppCardTemplateId_SelectListItems.FirstOrDefault()?.Text;
            }
        }

        public bool AppCardTemplateIdIsDisplayed => AppConfig.Settings.Feature_AppCardTemplate;

        #endregion AppCardTemplateId

        public IList<PlaceholderModel> Placeholders { get; set; } = new List<PlaceholderModel>();

        [DisplayName("Promotion activation required")]
        public bool PromotionActivationRequired { get; set; } = true;

        /// <remarks>
        /// ELP-8423: Abuse 'CampaignEmployee' role to determine if an issuer is allowed to set 'PromotionActivationRequired' field value.
        ///   Because the 'CampaignEmployee' isn't yet used for anything else and now we exclusively use it for this.
        /// </remarks>
        public bool PromotionActivationRequired_IsEditable(IPrincipal user) => user.IsInRole("CampaignEmployee");
    }
}