using Foolproof;
using MessagePack;
using nl.boxplosive.BackOffice.Mvc.Attributes;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Service.ServiceModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Loyalty
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class BasicStepModel : StepModelBase
    {
        public BasicStepModel()
        {
        }

        public BasicStepModel(LoyaltyCampaign campaign)
        {
            Title = campaign.Title;
            ProgramExternalId = campaign.ProgramExternalId;
            StampType = campaign.StampType;
            AppCardStackDefinitionId = campaign.AppCardStackDefinitionId;
            if (campaign.AppCardOrder.HasValue)
                AppCardOrder = campaign.AppCardOrder.Value;
            AppCardTemplateId = campaign.AppCardTemplateId;
            Description = campaign.Description;
            FontColor = campaign.FontColor;
            TermsConditions = campaign.TermsConditions;
            ImageUrl = campaign.ImageFileName;
            IsContinuousSaving = campaign.IsContinuousSaving;
            BalanceMessages = campaign.BalanceMessages;
            Placeholders = PlaceholderFactory.GetPlaceholderModels(AppCardTemplateId, campaign.PlaceholderValues);
        }

        public bool StampTypeEnabled { get; } = AppConfig.Settings.Feature_HybridSaving;
        public bool AppCardStackDefinitionsEnabled { get; } = AppConfig.Settings.Feature_FlexibleAppCardStacks;

        [Required]
        [StringLength(50, ErrorMessage = "Title has a maximum of 50 characters")]
        public string Title { get; set; }

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
                return TemplateFactory.CreateSelectList(TemplateType.Loyalty, AppCardTemplateId);
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

        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [Required]
        [ToStringLength(1000, ErrorMessage = "Description has a maximum of 1000 characters")]
        public HtmlEditor Description { get; set; }

        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [DisplayName("Terms and conditions")]
        public HtmlEditor TermsConditions { get; set; }

        [Required]
        [DisplayName("Font color")]
        public string FontColor { get; set; }

        public HashSet<string> FontColors = new HashSet<string> {
            "black",
            "white"
        };

        [DisplayName("Image")]
        [IgnoreMember]
        [RequiredIfEmpty("ImageUrl", ErrorMessage = "An image is required")]
        public HttpPostedFileBase Image { get; set; }

        public string ImageName { get; set; }

        public string ImageUrl
        {
            get
            {
                return _ImageUrl;
            }
            set
            {
                _ImageUrl = SdkConfig.Current.OverrideImageUrl(value);
            }
        }

        private string _ImageUrl;

        [DisplayName("Continuous saving")]
        public bool IsContinuousSaving { get; set; }

        [AllowHtml]
        [DisplayName("Loyalty points messages")]
        public string BalanceMessages { get; set; }

        public string BalanceMessagesPlaceholderText
        {
            get
            {
                return "{\n\t\"0\": \"Start saving\",\n\t\"1\": \"You saved 1 point already\"\n}";
            }
        }

        [Required]
        [DisplayName("Stamp type")]
        public StampType StampType { get; set; }

        [DisplayName("Program external ID")]
        [Required]
        [StringLength(100, ErrorMessage = "Program external ID has a maximum of 100 characters")]
        public string ProgramExternalId { get; set; }

        public string StampTypeText => LoyaltyFactory.StampTypeToDisplayText(StampType);

        public List<SelectListItem> StampTypes { get; set; } = BaseFactory.EnumToSelectList<StampType>(LoyaltyFactory.StampTypeToDisplayText);

        public IList<PlaceholderModel> Placeholders { get; set; } = new List<PlaceholderModel>();

        public void CopyValuesToServiceModel(LoyaltyCampaign serviceModel)
        {
            serviceModel.Title = Title;
            serviceModel.ProgramExternalId = ProgramExternalId;
            serviceModel.StampType = StampType;
            serviceModel.AppCardStackDefinitionId = AppCardStackDefinitionId;
            serviceModel.AppCardOrder = AppCardOrder;
            serviceModel.AppCardTemplateId = AppCardTemplateId;
            serviceModel.Description = Description;
            serviceModel.TermsConditions = TermsConditions;
            serviceModel.FontColor = FontColor;
            serviceModel.IsContinuousSaving = IsContinuousSaving;
            serviceModel.BalanceMessages = BalanceMessages;
            serviceModel.ImageFileName = ImageUrl;
            serviceModel.PlaceholderValues = Placeholders.Select(item => item.ToServiceModel()).ToList();
        }
    }
}