using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public abstract class StepModelBase
    {
        public bool? IsValid { get; set; }

        public List<string> ErrorFieldNames => new List<string>();
    }
}