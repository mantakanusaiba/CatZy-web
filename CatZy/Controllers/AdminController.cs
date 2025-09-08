using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.Mvc;
using Catzy.Models;


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

        public ActionResult DoctorApproval()
        {
            List<DoctorCredential> doctors = new List<DoctorCredential>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "SELECT * FROM DoctorCredentials WHERE Status = 'Pending'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();

                SqlDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    doctors.Add(new DoctorCredential
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        ConsultationHours = reader["ConsultationHours"].ToString(),
                        Experience = Convert.ToInt32(reader["Experience"]),
                        Certificates = reader["Certificates"].ToString(),
                        ProfilePic = reader["ProfilePic"].ToString(),
                        Status = reader["Status"].ToString()
                    });
                }
            }

            return View(doctors);
        }


        public ActionResult ApproveDoctor(int id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "UPDATE DoctorCredentials SET Status = 'Approved' WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("DoctorApproval");
        }

        public ActionResult RejectDoctor(int id)
        {
            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "UPDATE DoctorCredentials SET Status = 'Rejected' WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("DoctorApproval");
        }


    }
}
