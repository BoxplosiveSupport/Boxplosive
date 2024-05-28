using MessagePack;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ServiceModel.Campaign;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models.Placeholder
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PlaceholderImageModel : PlaceholderModel
    {
        public PlaceholderImageModel()
        {
        }

        public PlaceholderImageModel(DtoPlaceholder placeHolder, PlaceholderValue placeHolderValue = null)
            : base(placeHolder, placeHolderValue)
        {
        }

        public string Url
        {
            get
            {
                return Value;
            }
            set
            {
                Value = SdkConfig.Current.OverrideImageUrl(value);
            }
        }

        [IgnoreMember]
        public HttpPostedFileBase PostedFile { get; set; }
    }
}