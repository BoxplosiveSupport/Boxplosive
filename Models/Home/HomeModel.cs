using System.Collections.Generic;

namespace nl.boxplosive.BackOffice.Mvc.Models.Home
{
    public class HomeModel : ViewModelBase
    {
        public HomeModel()
        {
            Create = new CreateModule();
            Publish = new PublishModel();
            Analyse = new AnalyseModel();
            ClientModules = new List<ProcessModule>();
        }

        public ExplanationPanel Explanation { get; set; }

        public CreateModule Create { get; set; }
        public PublishModel Publish { get; set; }
        public AnalyseModel Analyse { get; set; }

        public List<ProcessModule> ClientModules { get; set; }
    }
}