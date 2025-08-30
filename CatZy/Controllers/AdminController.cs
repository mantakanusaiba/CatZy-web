using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class AdminController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Admin")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];
            return View();
        }
    }
}
