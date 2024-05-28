using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Management
{
    public class BackgroundProcessesModel : ViewModelBase
    {
        public BackgroundProcessesModel()
        {
            BackgroundProcesses = new List<BackgroundProcessModel>();
        }

        public List<BackgroundProcessModel> BackgroundProcesses { get; set; } 
    }
}