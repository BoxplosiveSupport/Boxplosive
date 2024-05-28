using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(nl.boxplosive.BackOffice.Mvc.Startup))]
namespace nl.boxplosive.BackOffice.Mvc
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
