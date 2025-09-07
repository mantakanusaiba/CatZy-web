using Catzy.Models;
using System.Linq;
using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class UserController : Controller
    {
        private AppDbContext db = new AppDbContext();

        [HttpGet]
        public ActionResult Appointment()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            return View(new Appointment());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Appointment(Appointment model)
        {
            if (ModelState.IsValid)
            {
                db.Appointments.Add(model);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(model);
        }

        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];
            return View();
        }
    }
        
}
