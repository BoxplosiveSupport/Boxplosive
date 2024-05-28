using Foolproof;
using MessagePack;
using nl.boxplosive.BackOffice.Mvc.Enums;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Models.Placeholder;
using nl.boxplosive.Configuration;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.BackOffice.Business.Data.Entities;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;
using nl.boxplosive.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlTypes;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.ManageNewsArticle
{
    public enum NewsArticleType
    {
        [Display(Name = "News Item")]
        News = 2,

        [Display(Name = "New Product")]
        NewProduct = 1,

        Campaign = 4,

        [Display(Name = "Folder Promotion")]
        Promotion = 3
    }

    [MessagePackObject(keyAsPropertyName: true)]
    public class NewsArticleModel : ViewModelBase
    {
        public NewsArticleModel()
        {
        }

        public NewsArticleModel(DtoNewsArticle dataModel)
        {
            Id = dataModel.Id;
            StartDate = dataModel.StartDate.ToLocalTimeInOperatingTimeZone();
            EndDate = dataModel.EndDate.ToLocalTimeInOperatingTimeZone();
            Title = dataModel.Title;
            Description = dataModel.Description;
            ImageUrl = dataModel.ImageUrl;
            Type = (NewsArticleType)(int)dataModel.Type;
            AppCardStackDefinitionId = dataModel.AppCardStackDefinitionId;
            AppCardOrder = dataModel.AppCardOrder;
            AppCardTemplateId = dataModel.AppCardTemplateId;
            Visibility = dataModel.Visibility.ConvertToUserVisibilityModel();
        }

        public DtoNewsArticle CopyValuesToDataModel()
        {
            if (StartDate < SqlDateTime.MinValue.Value.Date)
                StartDate = SqlDateTime.MinValue.Value.Date;

            if (EndDate < SqlDateTime.MinValue.Value.Date)
                EndDate = SqlDateTime.MinValue.Value.Date;

            var dataModel = new DtoNewsArticle();
            dataModel.Id = Id;
            dataModel.Title = Title;
            dataModel.Description = Description;
            dataModel.ImageUrl = ImageUrl;
            dataModel.Type = (DtoNewsArticleType)(int)Type;
            dataModel.StartDate = StartDate.ToUtcInOperatingTimeZone();
            dataModel.EndDate = EndDate.ToUtcEndOfDayInOperatingTimeZone();
            dataModel.AppCardStackDefinitionId = AppCardStackDefinitionId;
            dataModel.AppCardOrder = AppCardOrder;
            dataModel.AppCardTemplateId = AppCardTemplateId;
            dataModel.Visibility = Visibility.ConvertToString();

            return dataModel;
        }

        public void MapId(DtoNewsArticle dataModel)
        {
            Id = dataModel.Id;
        }

        public IList<DtoPlaceholderValue> ToDataObject_PlaceholderValues()
        {
            return Placeholders.Select(item => item.ToServiceModel().ToDataObject(newsArticleId: Id)).ToList();
        }

        public bool AppCardStackDefinition_DisplayField { get; } = AppConfig.Settings.Feature_FlexibleAppCardStacks;
        public bool Visibility_DisplayField { get; } = AppConfig.Settings.Feature_NewsArticleVisibility;

        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType(DataType.MultilineText)]
        [AllowHtml]
        public HtmlEditor Description { get; set; }

        [DisplayName("Image 01")]
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

        [Range(1, int.MaxValue)]
        public NewsArticleType Type { get; set; } = NewsArticleType.News;

        [DisplayName("Start date")]
        public DateTime StartDate { get; set; } = DateTime.Today;

        [DisplayName("End date")]
        [GreaterThanOrEqualTo(nameof(StartDate))]
        public DateTime EndDate { get; set; } = DateTime.Today.AddDays(7);

        [DisplayName("Image 01")]
        [IgnoreMember]
        [RequiredIfEmpty("ImageUrl", ErrorMessage = "An image is required")]
        public HttpPostedFileBase Image1Upload { get; set; }

        public List<SelectListItem> AppCardStackDefinitions => AppCardStackDefinitionFactory.AppCardStackDefinitionToSelectList(AppCardStackDefinitionId);

        [Display(Name = "AppCard stack")]
        [RequiredIfTrue(nameof(AppCardStackDefinition_DisplayField), ErrorMessage = "The AppCard stack field is required.")]
        public int AppCardStackDefinitionId { get; set; }

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
                return TemplateFactory.CreateSelectList(TemplateType.NewsArticle, AppCardTemplateId);
            }
        }

        public bool AppCardTemplateIdIsDisplayed => AppConfig.Settings.Feature_AppCardTemplate;

        #endregion AppCardTemplateId

        public IList<PlaceholderModel> Placeholders { get; set; } = new List<PlaceholderModel>();

        [DisplayName("Visibility")]
        public UserVisibilityModel Visibility { get; set; }
    }
}