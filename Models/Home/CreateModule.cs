using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class CreateModule
    {
        public CreateModule()
        {
            CreateActions = new List<ProcessModule>();
        }

        public List<ProcessModule> CreateActions { get; set; } 
    }
}