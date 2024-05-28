using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class LogModel
    {
        public string TimeStamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
    }
}
