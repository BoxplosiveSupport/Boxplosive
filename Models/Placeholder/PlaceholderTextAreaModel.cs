using System.Web.Mvc;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.Sdk;
using nl.boxplosive.Service.ServiceModel.Campaign;

namespace nl.boxplosive.BackOffice.Mvc.Models.Placeholder
{
    public class PlaceholderTextAreaModel : PlaceholderModel
    {
        public PlaceholderTextAreaModel()
        {
        }

        public PlaceholderTextAreaModel(DtoPlaceholder placeHolder, PlaceholderValue placeHolderValue = null)
            : base(placeHolder, placeHolderValue)
        {
        }

        [AllowHtml]
        public override string Value
        {
            get { return _Value; }
            set { _Value = DataHelper.Sanitize(value); }
        }

        private string _Value;
    }
}