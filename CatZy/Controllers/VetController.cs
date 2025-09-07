using System.Linq;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class VetController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Credentials()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];
            return View(); 
        }

        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];

            var appointments = db.Appointments.ToList();
            return View(appointments);
        }


        public ActionResult Appointment()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            ViewBag.User = Session["Username"];

            var appointments = db.Appointments.ToList();
            return View(appointments);
        }

    }
}
