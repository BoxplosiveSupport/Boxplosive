using System;

namespace nl.boxplosive.BackOffice.Mvc.Models.Reclamation
{
    public class MutationModel
    {
        public DateTime DateIssued { get; set; }

        public int MutationValue { get; set; }

        // TODO: Convert to correct sources enum
        public string Source { get; set; }

        public string Comment { get; set; }

        public bool? PossitiveMutation
        {
            get
            {
                if (MutationValue > 0)
                    return true;
                if (MutationValue < 0)
                    return false;

                return null;
            }
        }
    }
}