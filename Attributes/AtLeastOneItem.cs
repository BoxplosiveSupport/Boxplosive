using System.Collections;
using System.ComponentModel.DataAnnotations;

namespace nl.boxplosive.BackOffice.Mvc.Attributes
{
    public class AtLeastOneItem : ValidationAttribute
    {
        private readonly int _minElements;

        public AtLeastOneItem(int minElements = 1)
        {
            _minElements = minElements;
        }

        public override bool IsValid(object value)
        {
            var list = value as IList;
            if (list != null)
            {
                return list.Count >= _minElements;
            }
            return false;
        }
    }
}