using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace nl.boxplosive.BackOffice.Mvc.Models
{
    public abstract class VersionedViewModelBase : ViewModelBase
    {
        public int Id { get; set; }

        public byte[] Version { get; set; }

        public bool IsNew
        {
            get
            {
                return Id == 0;
            }
        }

        public string VersionText
        {
            get
            {
                return Convert.ToBase64String(Version);
            }
            set
            {
                Version = Convert.FromBase64String(value);
            }
        }
    }
}