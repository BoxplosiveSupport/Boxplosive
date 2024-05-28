using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Sdk.Enums;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class TemplateEditModel : TemplateModelBase
    {
        public TemplateEditModel()
            : base()
        {
        }

        public TemplateEditModel(int id)
            : this()
        {
            DtoTemplate template = TemplateRepository.Get(id);

            Id = template.Id;
            Version = template.Version;
            Title = template.Title;
            Type = template.Type;
            Json = template.Json;
        }
    }
}