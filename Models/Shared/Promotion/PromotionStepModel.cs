using Foolproof;
using MessagePack;
using nl.boxplosive.Sdk;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models.Shared.Promotion
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PromotionStepModel : StepModelBase, IValidatableObject
    {
        public PromotionStepModel()
        {
        }

        #region Image

        [DisplayName("Image")]
        [IgnoreMember]
        [RequiredIfEmpty("ImageUrl", ErrorMessage = "The Image field is required.")]
        public HttpPostedFileBase Image { get; set; }

        public string ImageName { get; set; }

        public string ImageUrl
        {
            get => _ImageUrl;
            set => _ImageUrl = SdkConfig.Current.OverrideImageUrl(value);
        }

        private string _ImageUrl;

        #endregion Image

        #region ImageEmail

        [DisplayName("Email image")]
        [IgnoreMember]
        public HttpPostedFileBase ImageEmailPostedFile { get; set; }

        public string ImageEmailName { get; set; }

        public string ImageEmail
        {
            get => _ImageEmail;
            set => _ImageEmail = SdkConfig.Current.OverrideImageUrl(value);
        }

        private string _ImageEmail;

        #endregion ImageEmail

        [DisplayName("Use different image for emails")]
        public bool ImageEmailEnabled
        {
            get
            {
                if (!_ImageEmailEnabled.HasValue)
                    _ImageEmailEnabled = ImageEmail != null && ImageEmail != ImageUrl;

                return _ImageEmailEnabled.Value;
            }
            set => _ImageEmailEnabled = value;
        }

        public bool? _ImageEmailEnabled { get; set; }

        public bool ImageEmailRequired => ImageEmailEnabled && string.IsNullOrEmpty(ImageEmail);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ImageEmailEnabled && string.IsNullOrEmpty(ImageEmail))
                yield return new ValidationResult("The Email image field is required.", new string[] { nameof(ImageEmailPostedFile) });
        }
    }
}