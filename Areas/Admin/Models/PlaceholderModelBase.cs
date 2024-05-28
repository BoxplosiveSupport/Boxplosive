using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using nl.boxplosive.BackOffice.Mvc.Models;
using nl.boxplosive.Business.Sdk.Template;
using nl.boxplosive.Data.Sdk.Template;

namespace nl.boxplosive.BackOffice.Mvc.Areas.Admin.Models
{
    public class PlaceholderModelBase : VersionedViewModelBase
    {
        public static List<string> MappedRegularPlaceholders = new List<string> {
            PlaceholderSubstitution.AppCardStackDisplayNamePlaceholder,
            PlaceholderSubstitution.AppCardStackPlaceholder,
            PlaceholderSubstitution.DeeplinkUrlPlaceholder,
            PlaceholderSubstitution.DescriptionPlaceholder,
            PlaceholderSubstitution.ExternalSupplierCouponId,
            PlaceholderSubstitution.ExternalSupplierCouponRedeemCode,
            PlaceholderSubstitution.ExternalSupplierCouponRedeemUrl,
            PlaceholderSubstitution.ImageUrlPlaceholder,
            PlaceholderSubstitution.TermsPlaceholder,
        };

        public PlaceholderModelBase()
        {
        }

        [Required]
        [RegularExpression("^[a-zA-Z0-9_]*$", ErrorMessage = "FieldName can only contain alphanumeric characters or underscores")]
        public string FieldName { get; set; }

        [Required]
        public string Label { get; set; }

        [Required]
        public PlaceholderDataType? DataType { get; set; }

        public IList<SelectListItem> DataTypes => FilterDataTypes(
            DataType.HasValue ? EnumHelper.GetSelectList(typeof(PlaceholderDataType), DataType) : EnumHelper.GetSelectList(typeof(PlaceholderDataType))
        );

        public void CreateOrEdit()
        {
            var dto = ToDto();
            PlaceholderRepository.InsertOrUpdate(dto);
        }

        public bool FieldNameAlreadyExists()
        {
            return PlaceholderRepository.GetAll().Any(p => p.FieldName == FieldName);
        }

        public bool FieldNameUsesMappedPlaceholder()
        {
            return MappedRegularPlaceholders.Any(p => p == FieldName) || FieldName.StartsWith(PlaceholderSubstitution.CustomerTokenPlaceholder);
        }

        private DtoPlaceholder ToDto()
        {
            return new DtoPlaceholder(Id, Version, FieldName, Label, DataType.Value);
        }

        private IList<SelectListItem> FilterDataTypes(IList<SelectListItem> items)
        {
            return items.Where(item => int.Parse(item.Value) != (int)PlaceholderDataType.MappedToProperty).ToList();
        }
    }
}