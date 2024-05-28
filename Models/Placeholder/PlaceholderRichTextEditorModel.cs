using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.Service.ServiceModel.Campaign;
using System;
using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc.Models.Placeholder
{
    public class PlaceholderRichTextEditorModel : PlaceholderModel
    {
        public PlaceholderRichTextEditorModel()
        {
        }

        public PlaceholderRichTextEditorModel(DtoPlaceholder placeHolder, PlaceholderValue placeHolderValue = null)
            : base(placeHolder, placeHolderValue)
        {
            _Value = HtmlEditor.Create(placeHolderValue?.Value, sanitizeHtml: false)?.Html;
        }

        [AllowHtml]
        public override string Value
        {
            get { return _Value; }
            set
            {
                try
                {
                    _Value = HtmlEditor.Create(value, allowExternalImages: false)?.Html;
                    InputValidationErrorMessage = null;
                }
                catch (BadImageFormatException)
                {
                    _Value = value;
                    InputValidationErrorMessage = "External images are not allowed";
                }
            }
        }

        private string _Value;
    }
}