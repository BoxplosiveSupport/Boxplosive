using MessagePack;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.Service.ServiceModel.Campaign;

namespace nl.boxplosive.BackOffice.Mvc.Models.Placeholder
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class PlaceholderModel
    {
        public PlaceholderModel()
        {
        }

        public PlaceholderModel(DtoPlaceholder placeHolder, PlaceholderValue placeHolderValue = null)
        {
            Id = placeHolderValue?.Id ?? 0;
            Version = placeHolderValue?.Version;
            PlaceholderId = placeHolderValue?.PlaceholderId ?? placeHolder.Id;
            Value = placeHolderValue?.Value;
            Placeholder = placeHolder;
        }

        public int Id { get; set; }
        public byte[] Version { get; set; }
        public int PlaceholderId { get; set; }
        public virtual string Value { get; set; }

        public DtoPlaceholder Placeholder { get; set; }

        /// <summary>
        /// Set this when user input is not valid.
        /// </summary>
        public string InputValidationErrorMessage { get; protected set; }

        public PlaceholderValue ToServiceModel()
        {
            return new PlaceholderValue()
            {
                Id = Id,
                Version = Version,
                PlaceholderId = PlaceholderId,
                Value = Value
            };
        }
    }
}