using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using System.Web.Routing;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Data.Sdk;
using nl.boxplosive.Data.Sdk.Template;
using PagedList;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class PlaceholderCreateModel : PlaceholderModelBase
    {
        public PlaceholderCreateModel()
            : base()
        {
        }
    }
}