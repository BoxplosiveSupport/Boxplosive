using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            var result = string.Join(" ", fi
                                              .GetCustomAttributes(typeof(DisplayAttribute), false)
                                              .Select(c => ((DisplayAttribute)c).Name)
                                              .ToArray());

            return !string.IsNullOrWhiteSpace(result)
                ? result
                : value.ToString();
        }


        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.ToString();
        }

        public static IList<string> SplitEnumValuesToString(this Enum input, byte valuesToSplit)
        {
            List<string> result = new List<string>();

            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                {
                    result.Add(value.ToString());
                }
            }

            return result;
        }

        public static IList<Enum> SplitEnumValuesToEnum(this Enum input, byte valuesToSplit)
        {
            List<Enum> result = new List<Enum>();

            foreach (Enum value in Enum.GetValues(input.GetType()))
            {
                if (input.HasFlag(value))
                {
                    result.Add(value);
                }
            }

            return result;
        }
    }
}