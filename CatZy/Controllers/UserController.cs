using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];
            return View();
        }
    }
}
