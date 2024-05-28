using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Business.Sdk.Template;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Data.Sdk.Template;
using nl.boxplosive.BackOffice.Business.AppCard.Entities;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class PlaceholderIndexModel : PlaceholderModelBase
    {
        public PlaceholderIndexModel()
        {
            Placeholders = PlaceholderRepository.GetAll();

            // Get regular mapped placeholders and add custom customertoken placeholder
            var mappedPlaceholderNames = MappedRegularPlaceholders.Concat(new List<string> { PlaceholderSubstitution.CustomerTokenPlaceholderLabel }).OrderBy(p => p);

            foreach (var mapped in mappedPlaceholderNames)
            {
                Placeholders.Add(new DtoPlaceholder() { FieldName = mapped, DataType = PlaceholderDataType.MappedToProperty });
            }
        }

        public IList<DtoPlaceholder> Placeholders;
    }
}