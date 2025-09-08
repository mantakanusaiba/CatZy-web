using Catzy.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class AccountController : Controller
    {
        private AppDbContext db = new AppDbContext();

        
        public ActionResult Signup()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Signup(User model)
        {
            if (ModelState.IsValid)
            {
               
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
                {
                   
                    string status = null;
                    using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                    {
                        string query = "SELECT TOP 1 Status FROM DoctorCredentials WHERE Email = @Email ORDER BY Id DESC";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@Email", user.Email);
                        conn.Open();
                        status = cmd.ExecuteScalar()?.ToString();
                    }

                    if (status == "Approved")
                    {
                        return RedirectToAction("Index", "Vet"); 
                    }
                    else
                    {
                        return RedirectToAction("Credentials", "Vet"); 
                    }
                }
                else
                    return RedirectToAction("Index", "User");
            }
            ViewBag.Message = "Invalid Email or Password.";
            return View(model);
        }



        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
