using System;
using System.ComponentModel.DataAnnotations;
using nl.boxplosive.Data.Sdk;

namespace nl.boxplosive.BackOffice.Mvc.Enums
{
    public enum StockStatusModel
    {
        /// <remarks>default</remarks>
        [Display(Name = "In stock")]
        InStock,

        [Display(Name = "Out of stock")]
        OutOfStock,

        [Display(Name = "Temporary out of stock")]
        TempOutOfStock
    }

    public static class StockStatusModelExtensions
    {
        public static StockStatus ConvertFromModel(this StockStatusModel value)
        {
            return (StockStatus)Enum.Parse(typeof(StockStatus), value.ToString());
        }

        public static StockStatusModel ConvertToModel(this StockStatus value)
        {
            return (StockStatusModel)Enum.Parse(typeof(StockStatusModel), value.ToString());
        }
    }
}