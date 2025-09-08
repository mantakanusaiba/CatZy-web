using System.Web.Mvc;
using Catzy.Models;
using System.Collections.Generic;

namespace Catzy.Controllers
{
    public class RescueController : Controller
    {
       
        private static List<RescueRequest> rescueRequests = new List<RescueRequest>();

        // GET: Rescue/Report
        public ActionResult Report()
        {
            return View();
        }

        // POST: Rescue/Report
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Report(RescueRequest request)
        {
            if (ModelState.IsValid)
            {
                
                request.Id = rescueRequests.Count + 1;
                rescueRequests.Add(request);

              
                TempData["SuccessMessage"] = "Rescue request submitted successfully!";

               
                return RedirectToAction("Index", "User");
            }

            return View(request);
        }

        // Show all reports with map
        public ActionResult AllReports()
        {
            return View(rescueRequests);
        }
    }
}
