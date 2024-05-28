using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Helpers
{
    public class ValidationHelper
    {
        public const string ValueAtLeast1Message = "The value must be at least 1";
        public const string ValueCannotBeZero = "The value cannot be 0";
        public const string ValueCannotBeNegative = "The value cannot be negative";
        public const string ValueMustBeAnInteger = "The value must be a valid integer";

        public static string OccRecordModifiedMessageFormat =
            "{0} failed because the {1} was already modified or deleted. Please go back and try editing again.";

        public static string GetOccRecordModifiedMessage(OccValidationAction action, string record)
        {
            return string.Format(OccRecordModifiedMessageFormat, action, record);
        }
    }

    // All actions in the form of present participle: -ing
    public enum OccValidationAction
    {
        Approving,
        Declining,
        Deleting,
        Saving,
        Readying
    }
}