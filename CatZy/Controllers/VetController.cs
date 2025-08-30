using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class VetController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];
            return View();
        }
    }
}
