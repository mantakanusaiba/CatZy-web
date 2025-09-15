using Catzy.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Catzy.Controllers
{
    public class UserController : Controller
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private AppDbContext db = new AppDbContext(); 

        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");
            ViewBag.User = Session["Username"];
            LoadNotificationsForUser(); // Load notifications for dashboard
            return View();
        }

        [HttpGet]
        public ActionResult Appointment()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            List<DoctorCredential> vets = new List<DoctorCredential>();

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "SELECT * FROM DoctorCredentials WHERE Status = 'Approved'";
                SqlCommand cmd = new SqlCommand(query, conn);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    vets.Add(new DoctorCredential
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        Email = reader["Email"].ToString(),
                        Phone = reader["Phone"].ToString(),
                        HospitalName = reader["HospitalName"] != DBNull.Value ? reader["HospitalName"].ToString() : "Not Provided",

                        Experience = (int)reader["Experience"],
                        ProfilePic = reader["ProfilePic"].ToString(),
                        ConsultationHours = reader["ConsultationHours"].ToString()
                    });
                }
            }

            return View(vets);
        }


        [HttpGet]
        public ActionResult BookAppointment(int doctorId)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            DoctorCredential doctor = null;

            using (var conn = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            {
                string query = "SELECT * FROM DoctorCredentials WHERE Id = @Id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@Id", doctorId);
                conn.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    doctor = new DoctorCredential
                    {
                        Id = (int)reader["Id"],
                        Name = reader["Name"].ToString(),
                        Specialization = reader["Specialization"].ToString(),
                        ConsultationHours = reader["ConsultationHours"].ToString()
                    };
                }
            }

            Appointment model = new Appointment
            {
                DoctorName = doctor?.Name,
                Specialization = doctor?.Specialization,
                ConsultationHours = doctor?.ConsultationHours
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BookAppointment(Appointment model)
        {
            if (ModelState.IsValid)
            {
                DateTime selectedDate = model.Date.Date;
                List<string> availableSlots = new List<string>();


                if (model.ConsultationHours.Contains("Morning"))
                {
                    availableSlots = new List<string> { "10:00 AM", "10:20 AM", "10:40 AM", "11:00 AM" };
                }
                else if (model.ConsultationHours.Contains("Evening"))
                {
                    availableSlots = new List<string> { "6:00 PM", "6:20 PM", "6:40 PM", "7:00 PM", "7:20 PM" };
                }

                int existingAppointmentsCount = db.Appointments.Count(a => a.Date == selectedDate && a.DoctorName == model.DoctorName);

                if (existingAppointmentsCount >= availableSlots.Count)
                {
                    TempData["Message"] = "Sorry! This doctor is fully booked for the selected date.";
                    return RedirectToAction("BookAppointment", new { doctorId = model.Id });
                }

                string assignedTime = availableSlots[existingAppointmentsCount];

                db.Appointments.Add(model);
                db.SaveChanges();

                TempData["Message"] = $"Appointment successfully booked for {model.DoctorName} at {assignedTime}";

                return RedirectToAction("BookAppointment", new { doctorId = model.Id });
            }

            return View(model);
        }


        [HttpGet]
        public JsonResult CheckAvailability(string doctorName, string date)
        {
            if (string.IsNullOrEmpty(date) || string.IsNullOrEmpty(doctorName))
                return Json(new { available = true }, JsonRequestBehavior.AllowGet);

            if (!DateTime.TryParse(date, out DateTime selectedDate))
                return Json(new { available = true }, JsonRequestBehavior.AllowGet);

            int count = db.Appointments.Count(a => a.Date == selectedDate.Date && a.DoctorName == doctorName);
            bool available = count < 3;

            return Json(new { available = available }, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public ActionResult AppointmentList()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            var appointments = db.Appointments.ToList();
            return View(appointments);
        }

        // COMMUNITY-ONLY Adoption with filtering
        public ActionResult Adoption(string age = "All", string gender = "All", string color = "All", string breed = "All")
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            var posts = new List<Catzy.Models.CatPost>();
            using (var conn = new SqlConnection(ConnStr))
            {
                // Build filter SQL and parameters
                var sql = @"
                    SELECT Id, Name, Age, Gender, Color, Breed, Description, ImageUrl, PostedBy, PostedAt, Status
                    FROM dbo.CatPosts
                    WHERE Status = N'Approved'";
                var conditions = new List<string>();
                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (!string.Equals(age, "All", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("Age = @Age");
                    cmd.Parameters.AddWithValue("@Age", age);
                }
                if (!string.Equals(gender, "All", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("Gender = @Gender");
                    cmd.Parameters.AddWithValue("@Gender", gender);
                }
                if (!string.Equals(color, "All", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("Color = @Color");
                    cmd.Parameters.AddWithValue("@Color", color);
                }
                if (!string.Equals(breed, "All", StringComparison.OrdinalIgnoreCase))
                {
                    conditions.Add("Breed = @Breed");
                    cmd.Parameters.AddWithValue("@Breed", breed);
                }

                if (conditions.Count > 0) sql += " AND " + string.Join(" AND ", conditions);
                sql += " ORDER BY PostedAt DESC;";
                cmd.CommandText = sql;
                conn.Open();

                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        posts.Add(new Catzy.Models.CatPost
                        {
                            Id = (int)r["Id"],
                            Name = Convert.ToString(r["Name"]),
                            Age = Convert.ToString(r["Age"]),
                            Gender = Convert.ToString(r["Gender"]),
                            Color = Convert.ToString(r["Color"]),
                            Breed = Convert.ToString(r["Breed"]),
                            Description = Convert.ToString(r["Description"]),
                            ImageUrl = Convert.ToString(r["ImageUrl"]),
                            PostedBy = Convert.ToString(r["PostedBy"]),
                            PostedAt = Convert.ToDateTime(r["PostedAt"]),
                            Status = Convert.ToString(r["Status"])
                        });
                    }
                }
            }

            ViewBag.SelectedAge = age;
            ViewBag.SelectedGender = gender;
            ViewBag.SelectedColor = color;
            ViewBag.SelectedBreed = breed;

            LoadNotificationsForUser(); // Load notifications for adoption page

            return View("Adoption", posts);
        }

        // COMMUNITY details
        public ActionResult PostCatDetails(int id)
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name, Age, Gender, Color, Breed, Description, ImageUrl
                FROM dbo.CatPosts
                WHERE Id = @Id AND Status = N'Approved';", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return HttpNotFound();
                    var post = new Catzy.Models.CatPost
                    {
                        Id = (int)r["Id"],
                        Name = Convert.ToString(r["Name"]),
                        Age = Convert.ToString(r["Age"]),
                        Gender = Convert.ToString(r["Gender"]),
                        Color = Convert.ToString(r["Color"]),
                        Breed = Convert.ToString(r["Breed"]),
                        Description = Convert.ToString(r["Description"]),
                        ImageUrl = Convert.ToString(r["ImageUrl"])
                    };
                    return View("PostCatDetails", post);
                }
            }
        }

        // COMMUNITY adopt bridge → reuse Adopt.cshtml by mapping CatPost → Cat
        public ActionResult AdoptPostCat(int id)
        {
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT Id, Name, Age, Gender, Color, Breed, ImageUrl
                FROM dbo.CatPosts
                WHERE Id = @Id AND Status = N'Approved';", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (!r.Read()) return HttpNotFound();
                    var cat = new Catzy.Models.Cat
                    {
                        Id = (int)r["Id"], // reuse post id
                        Name = Convert.ToString(r["Name"]),
                        Breed = Convert.ToString(r["Breed"]),
                        Gender = Convert.ToString(r["Gender"]),
                        Age = Convert.ToString(r["Age"]),
                        Color = Convert.ToString(r["Color"]),
                        ImageUrl = Convert.ToString(r["ImageUrl"]),
                        Weight = "",
                        FavoriteToy = "",
                        FavoriteTreat = "",
                        FavoriteActivity = ""
                    };
                    return View("Adopt", cat);
                }
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAdoption(AdoptionViewModel model)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                // try to repopulate from community post
                using (var conn = new SqlConnection(ConnStr))
                using (var cmd = new SqlCommand(@"
                    SELECT Id, Name, Age, Gender, Color, Breed, ImageUrl
                    FROM dbo.CatPosts
                    WHERE Id = @Id AND Status = N'Approved';", conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.CatId);
                    conn.Open();
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            var catPost = new Cat
                            {
                                Id = (int)r["Id"],
                                Name = Convert.ToString(r["Name"]),
                                Breed = Convert.ToString(r["Breed"]),
                                Gender = Convert.ToString(r["Gender"]),
                                Age = Convert.ToString(r["Age"]),
                                Color = Convert.ToString(r["Color"]),
                                ImageUrl = Convert.ToString(r["ImageUrl"])
                            };
                            return View("Adopt", catPost);
                        }
                    }
                }
                return RedirectToAction("Adoption");
            }

            // Resolve cat name from community posts
            string catName = null;
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                SELECT Name FROM dbo.CatPosts
                WHERE Id = @Id AND Status = N'Approved';", conn))
            {
                cmd.Parameters.AddWithValue("@Id", model.CatId);
                conn.Open();
                var o = cmd.ExecuteScalar();
                if (o != null && o != DBNull.Value) catName = Convert.ToString(o);
            }

            if (catName == null) return HttpNotFound();

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.AdoptionRequests
                    (CatId, CatName, ApplicantName, Email, Phone, Address, Status, SubmittedAt)
                VALUES
                    (@CatId, @CatName, @ApplicantName, @Email, @Phone, @Address, N'Pending', SYSDATETIME());", conn))
            {
                cmd.Parameters.AddWithValue("@CatId", model.CatId);
                cmd.Parameters.AddWithValue("@CatName", catName);
                cmd.Parameters.AddWithValue("@ApplicantName", model.FullName);
                cmd.Parameters.AddWithValue("@Email", model.Email);
                cmd.Parameters.AddWithValue("@Phone", model.Phone);
                cmd.Parameters.AddWithValue("@Address", model.Address);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["AdoptMessage"] = $"Thanks {model.FullName}! Your adoption request for {catName} was submitted.";
            return RedirectToAction("AdoptionConfirmation");
        }

        public ActionResult AdoptionConfirmation()
        {
            ViewBag.Message = TempData["AdoptMessage"];
            return View();
        }

        [HttpGet]
        public ActionResult PostCat()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult PostCat(string Name, string Age, string Gender, string Color, string Breed, string Description, HttpPostedFileBase ImageFile)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            string imagePath = "~/Content/image/cat-placeholder.jpg";
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                var uploads = Server.MapPath("~/Content/Uploads");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                var fileName = $"{DateTime.UtcNow.Ticks}_{Path.GetFileName(ImageFile.FileName)}";
                var fullPath = Path.Combine(uploads, fileName);
                ImageFile.SaveAs(fullPath);
                imagePath = $"~/Content/Uploads/{fileName}";
            }

            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                INSERT INTO dbo.CatPosts
                    (Name, Age, Gender, Color, Breed, Description, ImageUrl, PostedBy, PostedAt, Status)
                VALUES
                    (@Name, @Age, @Gender, @Color, @Breed, @Description, @ImageUrl, @PostedBy, SYSDATETIME(), N'Pending');", conn))
            {
                cmd.Parameters.AddWithValue("@Name", Name ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Age", (object)Age ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Gender", (object)Gender ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Color", (object)Color ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Breed", (object)Breed ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", (object)Description ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ImageUrl", (object)imagePath ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PostedBy", Convert.ToString(Session["Username"]) ?? (object)DBNull.Value);
                conn.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["AdoptMessage"] = $"Cat '{Name}' submitted for review. It will be visible after admin approval.";
            return RedirectToAction("Adoption");
        }

        public ActionResult PetDiary()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");
            ViewBag.User = Session["Username"];
            return View("PetDiary");
        }

        public ActionResult AboutUs() => View("AboutUs");

        // Load notifications for current user
        private void LoadNotificationsForUser()
        {
            var username = Convert.ToString(Session["Username"]);
            if (string.IsNullOrWhiteSpace(username)) return;

            var notes = new List<NotificationViewModel>(); // Changed from List<dynamic>
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
        SELECT Id, Title, Message, CreatedAt
        FROM dbo.Notifications
        WHERE Username = @Username AND IsRead = 0
        ORDER BY CreatedAt DESC;", conn))
            {
                cmd.Parameters.AddWithValue("@Username", username);
                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        notes.Add(new NotificationViewModel // Changed from anonymous object
                        {
                            Id = (int)r["Id"],
                            Title = Convert.ToString(r["Title"]),
                            Message = Convert.ToString(r["Message"]),
                            CreatedAt = Convert.ToDateTime(r["CreatedAt"])
                        });
                    }
                }
            }
            ViewBag.Notifications = notes;
        }
        public class NotificationViewModel
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Message { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // Dismiss notification
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DismissNotification(int id)
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account");

            var username = Convert.ToString(Session["Username"]);
            using (var conn = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
                UPDATE dbo.Notifications
                SET IsRead = 1
                WHERE Id = @Id AND Username = @Username;", conn))
            {
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@Username", username);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            return RedirectToAction("Index");
        }
    }
    // Add this to your Models folder or in your UserController
    

}
