using System.Web.Mvc;

namespace nl.boxplosive.BackOffice.Mvc
{
    [AllowAnonymous]
    public partial class ErrorController : Controller
	{
		public virtual ActionResult Index()
		{
			return Error500();
		}

        public virtual ViewResult Error403()
		{
			return View();
		}

        public virtual ViewResult Error404()
		{
			return View();
		}

        public virtual ViewResult Error500()
		{
			return View();
		}
	}
}