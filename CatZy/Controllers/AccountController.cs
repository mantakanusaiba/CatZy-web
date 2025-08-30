using Catzy.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        // GET: Signup
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(User model)
        {
            if (ModelState.IsValid)
            {
                // check if email already exists
                var existing = db.Users.FirstOrDefault(u => u.Email == model.Email);
                if (existing != null)
                {
                    ViewBag.Message = "Email already registered!";
                    return View(model);
                }

                db.Users.Add(model);
                db.SaveChanges();
                ViewBag.Message = "Signup successful! Please login.";
                return RedirectToAction("Login");
            }
            return View(model);
        }

        // GET: Login
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Login(User model)
        {
            var user = db.Users.FirstOrDefault(u => u.Email == model.Email && u.Password == model.Password);
            if (user != null)
            {
                Session["Username"] = user.Username;
                Session["Email"] = user.Email;
                Session["Role"] = user.Role;

                if (user.Role == "Admin")
                    return RedirectToAction("Index", "Admin");
                else if (user.Role == "Vet")
                    return RedirectToAction("Index", "Vet");
                else
                    return RedirectToAction("Index", "User");
            }
            ViewBag.Message = "Invalid Email or Password.";
            return View(model);
        }

        // Logout
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
