using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public abstract class TemplateModelBase : VersionedViewModelBase
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public TemplateType? Type { get; set; }

        public IList<SelectListItem> Types => Type.HasValue
            ? EnumHelper.GetSelectList(typeof(TemplateType), Type)
            : EnumHelper.GetSelectList(typeof(TemplateType));

        [AllowHtml]
        [DataType(DataType.MultilineText)]
        [Required]
        public string Json { get; set; }

        public TemplateModelBase()
            : base()
        {
        }

        public void CreateOrEdit()
        {
            var dto = ToDto();
            TemplateRepository.InsertOrUpdate(dto);
        }

        private DtoTemplate ToDto()
        {
            return new DtoTemplate(Id, Version, Title, Type.Value, Json);
        }
    }
}