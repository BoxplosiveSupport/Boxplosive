using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Factories;
using nl.boxplosive.BackOffice.Mvc.Helpers;
using nl.boxplosive.Business.Sdk.Entities;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Models.NotificationDefinition
{
    public class NotificationDefinitionModel : VersionedViewModelBase
    {
        public NotificationDefinitionModel()
        {
        }

        public NotificationDefinitionModel(IDtoNotificationDefinition notificationDefinition)
        {
            if (notificationDefinition != null)
            {
                Id = notificationDefinition.Id;
                Version = notificationDefinition.Version;
                Name = notificationDefinition.Name;
                Type = EnumHelper<NotificationType>.ConvertOrDefault(notificationDefinition.NotificationType);
                CustomEvent = notificationDefinition.CustomEvent;
                Arguments = notificationDefinition.Arguments;
            }
        }

        [Required]
        public string Name { get; set; }

        public List<SelectListItem> NotificationTypes { get; set; }
            = BaseFactory.EnumToSelectList<NotificationType>(NotificationDefinitionFactory.NotificationTypeToDisplayText);

        [Required]
        public NotificationType? Type { get; set; }

        [DisplayName("Custom event")]
        public string CustomEvent { get; set; }

        [DataType(DataType.MultilineText)]
        public string Arguments { get; set; }
    }
}