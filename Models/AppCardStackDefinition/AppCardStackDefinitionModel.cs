using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Models.AppCardStackDefinition
{
    public class AppCardStackDefinitionModel : VersionedViewModelBase
    {
        public AppCardStackDefinitionModel()
        {
        }

        public AppCardStackDefinitionModel(IDtoAppCardStackDefinition item)
        {
            if (item == null)
                return;

            Id = item.Id;
            Version = item.Version;
            Name = item.Name;
            Order = item.Order;
            DisplayName = item.DisplayName;
            CardSize = Enum.TryParse(item.CardSize, ignoreCase: true, out AppCardStackDefinitionCardSize cardSize) 
                    ? cardSize 
                    : (AppCardStackDefinitionCardSize?)null;
        }

        // Not required: is set by POST method in controller (on submit of form)
        public string Name { get; set; }

        [Required]
        public int Order { get; set; }

        [DisplayName("Display name")]
        [Required]
        public string DisplayName { get; set; }

        [DisplayName("Card size")]
        [Required]
        public AppCardStackDefinitionCardSize? CardSize { get; set; } = AppCardStackDefinitionCardSize.Medium;
    }
}