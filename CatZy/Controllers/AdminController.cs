using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
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
                        HospitalName = reader["HospitalName"].ToString(),
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


        
        public ActionResult Adoptions(string status = "All", string search = "")
        {
            EnsureSeed();

            var list = (List<AdoptionRequest>)Session["AdoptionRequests"];

            if (!string.IsNullOrWhiteSpace(search))
            {
                list = list.Where(x =>
                    (x.ApplicantName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                 || (x.CatName ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                 || (x.Email ?? "").IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0
                ).ToList();
            }

            if (!string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
                list = list.Where(x => x.Status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();

            ViewBag.SelectedStatus = status;
            ViewBag.Search = search;
            ViewBag.CatPosts = (List<CatPost>)Session["CatPosts"];

            return View(list.OrderByDescending(x => x.SubmittedAt).ToList());
        } 

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            EnsureSeed();
            var list = (List<AdoptionRequest>)Session["AdoptionRequests"];
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null) item.Status = "Approved";
            TempData["Flash"] = $"Request #{id} approved.";
            return RedirectToAction("Adoptions");
        } 

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            EnsureSeed();
            var list = (List<AdoptionRequest>)Session["AdoptionRequests"];
            var item = list.FirstOrDefault(x => x.Id == id);
            if (item != null) list.Remove(item);
            TempData["Flash"] = $"Request #{id} deleted.";
            return RedirectToAction("Adoptions");
        } 

      
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(NewAdoptionForm form)
        {
            EnsureSeed();
            if (!ModelState.IsValid)
            {
                TempData["Flash"] = "Please fill in all required fields.";
                return RedirectToAction("Adoptions");
            }
            var list = (List<AdoptionRequest>)Session["AdoptionRequests"];
            var nextId = list.Any() ? list.Max(x => x.Id) + 1 : 1;
            list.Add(new AdoptionRequest
            {
                Id = nextId,
                CatName = form.CatName,
                ApplicantName = form.ApplicantName,
                Email = form.Email,
                Phone = form.Phone,
                Status = "Pending",
                SubmittedAt = DateTime.UtcNow
            });
            TempData["Flash"] = $"Request for {form.CatName} added.";
            return RedirectToAction("Adoptions");
        } 

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCat(string Name, string Age, string Gender, string Color, string Breed, string Description, HttpPostedFileBase ImageFile)
        {
            EnsureSeed();
            var cats = (List<CatPost>)Session["CatPosts"];

            string savedPath = "~/Content/image/cat-placeholder.jpg";
            if (ImageFile != null && ImageFile.ContentLength > 0)
            {
                var uploadsFolder = Server.MapPath("~/Content/Uploads");
                if (!System.IO.Directory.Exists(uploadsFolder))
                    System.IO.Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{DateTime.UtcNow.Ticks}_{System.IO.Path.GetFileName(ImageFile.FileName)}";
                var fullPath = System.IO.Path.Combine(uploadsFolder, fileName);
                ImageFile.SaveAs(fullPath);
                savedPath = $"~/Content/Uploads/{fileName}";
            } 

            var nextId = cats.Any() ? cats.Max(c => c.Id) + 1 : 1;
            cats.Add(new CatPost
            {
                Id = nextId,
                Name = Name,
                Age = Age,
                Gender = Gender,
                Color = Color,
                Breed = Breed,
                Description = Description,
                ImageUrl = savedPath
            });

            TempData["Flash"] = $"Cat '{Name}' posted for adoption.";
            return RedirectToAction("Adoptions");
        } 

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCat(int id)
        {
            EnsureSeed();
            var cats = (List<CatPost>)Session["CatPosts"];
            var item = cats.FirstOrDefault(c => c.Id == id);
            if (item != null) cats.Remove(item);
            TempData["Flash"] = $"Cat post #{id} removed.";
            return RedirectToAction("Adoptions");
        } 

        
        private void EnsureSeed()
        {
            if (Session["AdoptionRequests"] == null)
            {
                Session["AdoptionRequests"] = new List<AdoptionRequest>
                {
                    new AdoptionRequest{ Id=1, CatName="Larry",   ApplicantName="Aminah Rahman", Email="aminah@example.com", Phone="01711-123456", Status="Pending",  SubmittedAt=DateTime.UtcNow.AddHours(-8)},
                    new AdoptionRequest{ Id=2, CatName="Whisker", ApplicantName="Rafi Ahmed",    Email="rafi@example.com",   Phone="01822-987654", Status="Approved", SubmittedAt=DateTime.UtcNow.AddDays(-1)},
                    new AdoptionRequest{ Id=3, CatName="Milo",    ApplicantName="Nadia Karim",   Email="nadia@example.com",  Phone="01633-222333", Status="Pending",  SubmittedAt=DateTime.UtcNow.AddHours(-2)},
                };
            }
            if (Session["CatPosts"] == null)
            {
                Session["CatPosts"] = new List<CatPost>
                {
                    new CatPost{ Id=1, Name="Shadow", Age="6 months", Gender="Male",   Color="Black", Breed="Mixed",   Description="Playful and curious.", ImageUrl="~/Content/image/cat1.jpg"},
                    new CatPost{ Id=2, Name="Snow",   Age="2 years",  Gender="Female", Color="White", Breed="Persian", Description="Calm cuddle buddy.",  ImageUrl="~/Content/image/cat2.jpg"}
                };
            }


        }
        
    }
    public class AdoptionRequest
{
    public int Id { get; set; }
    public string CatName { get; set; }
    public string ApplicantName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string Status { get; set; } 
    public DateTime SubmittedAt { get; set; }
}

public class NewAdoptionForm
{
    public string CatName { get; set; }
    public string ApplicantName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}

public class CatPost
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Age { get; set; }
    public string Gender { get; set; }
    public string Color { get; set; }
    public string Breed { get; set; }
    public string Description { get; set; }
    public string ImageUrl { get; set; } 
}


}

