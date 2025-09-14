using Catzy.Models;
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class VetController : Controller
    {
        private AppDbContext db = new AppDbContext();

        public ActionResult Credentials()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }
        [HttpPost]
        public ActionResult Credentials(DoctorCredential model, HttpPostedFileBase Certificates, HttpPostedFileBase ProfilePic)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            model.ConsultationHours = Request.Form["ConsultationHours"];
            model.HospitalName = Request.Form["HospitalName"];

            if (ModelState.IsValid)
            {
                string certPath = null;
                string picPath = null;

                if (Certificates != null && Certificates.ContentLength > 0)
                {
                    certPath = "/Uploads/Certificates/" + Path.GetFileName(Certificates.FileName);
                    Certificates.SaveAs(Server.MapPath(certPath));
                }

                if (ProfilePic != null && ProfilePic.ContentLength > 0)
                {
                    picPath = "/Uploads/ProfilePics/" + Path.GetFileName(ProfilePic.FileName);
                    ProfilePic.SaveAs(Server.MapPath(picPath));
                }

                using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
                {
                    string query = @"INSERT INTO DoctorCredentials 
            ( Name, Email, Phone, HospitalName, Specialization, ConsultationHours, Experience, Certificates, ProfilePic, Status) 
             VALUES (@Name, @Email, @Phone, @HospitalName, @Specialization, @ConsultationHours, @Experience, @Certificates, @ProfilePic, 'Pending')";

                    SqlCommand cmd = new SqlCommand(query, conn);

                    cmd.Parameters.AddWithValue("@Name", model.Name);
                    cmd.Parameters.AddWithValue("@Email", model.Email);
                    cmd.Parameters.AddWithValue("@Phone", model.Phone);
                    cmd.Parameters.AddWithValue("@HospitalName", model.HospitalName); 
                    cmd.Parameters.AddWithValue("@Specialization", model.Specialization);
                    cmd.Parameters.AddWithValue("@ConsultationHours", model.ConsultationHours);
                    cmd.Parameters.AddWithValue("@Experience", model.Experience);
                    cmd.Parameters.AddWithValue("@Certificates", (object)certPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ProfilePic", (object)picPath ?? DBNull.Value);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }

                Session["CredentialsSubmitted"] = true;
                return RedirectToAction("Index");
            }

            return View(model);
        }


        [HttpGet]
        public ActionResult AppointmentList()
        {
           
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            
            string vetName = Session["Username"].ToString();

         
            var appointments = db.Appointments
                                 .Where(a => a.DoctorName == vetName)
                                 .OrderBy(a => a.Date)
                                 .ToList();

            return View(appointments);
        }


        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "Vet")
                return RedirectToAction("Login", "Account");

            string vetEmail = Session["Email"]?.ToString();
            if (string.IsNullOrEmpty(vetEmail))
                return RedirectToAction("Credentials");

            string status = null;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "SELECT TOP 1 Status FROM DoctorCredentials WHERE Email = @Email ORDER BY Id DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Email", vetEmail);
                conn.Open();
                status = cmd.ExecuteScalar()?.ToString();
            }

            if (status == null)
            {
                
                TempData["Message"] = "Please submit your credentials first.";
                return RedirectToAction("Credentials");
            }
            else if (status != "Approved")
            {
                
                TempData["Message"] = "Your credentials are not approved yet by Admin.";
                return RedirectToAction("Credentials");
            }

            
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
