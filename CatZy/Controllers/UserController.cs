using CatZy.Models;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class UserController : Controller
    {
        public ActionResult Index()
        {
            if (Session["Role"] == null || Session["Role"].ToString() != "User")
                return RedirectToAction("Login", "Account"); // ensure session is set elsewhere
            ViewBag.User = Session["Username"];
            return View("Index"); // explicit, optional
        } // [28]

        public ActionResult Adoption(string age = "All", string gender = "All", string color = "All", string breed = "All")
        {
            var cats = GetCats();

            if (age != "All")
                cats = cats.Where(c => c.Age == age).ToList();
            if (gender != "All")
                cats = cats.Where(c => c.Gender == gender).ToList();
            if (color != "All")
                cats = cats.Where(c => c.Color == color).ToList();
            if (breed != "All")
                cats = cats.Where(c => c.Breed == breed).ToList();

            ViewBag.SelectedAge = age;
            ViewBag.SelectedGender = gender;
            ViewBag.SelectedColor = color;
            ViewBag.SelectedBreed = breed;

            return View("Adoption", cats); // explicit, optional
        } // [28]

        public ActionResult CatDetails(int id)
        {
            var cat = GetCats().FirstOrDefault(c => c.Id == id);
            if (cat == null) return HttpNotFound();
            return View("CatDetails", cat); // explicit, optional
        } // [28]

        public ActionResult Adopt(int id)
        {
            var cat = GetCats().FirstOrDefault(c => c.Id == id);
            if (cat == null) return HttpNotFound();

            // Option 1: Named view (relies on Views/User/Adopt.cshtml)
            return View("Adopt", cat);

            // Option 2: Absolute path (uncomment to bypass discovery completely)
            // return View("~/Views/User/Adopt.cshtml", cat);
        } // [28][23]

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ConfirmAdoption(AdoptionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var cat = GetCats().FirstOrDefault(c => c.Id == model.CatId);
                // Re-render form with validation errors
                return View("Adopt", cat);
            }

            TempData["Message"] = $"Thank you {model.FullName}, you adopted cat ID {model.CatId}!";
            return RedirectToAction("AdoptionConfirmation");
        } // [28]

        public ActionResult AdoptionConfirmation()
        {
            return View("AdoptionConfirmation");
        } // [28]

        public ActionResult AboutUs()
        {
            return View("AboutUs");
        } // [28]

        private List<Cat> GetCats()
        {
            return new List<Cat>
            {
                new Cat { Id = 1, Name = "Larry", Breed = "Biman", Gender = "Female", Age = "1 year", Color = "Black", Weight = "7 pounds", FavoriteToy = "Yarn", FavoriteTreat = "Tuna Fish", FavoriteActivity = "Napping", ImageUrl = "~/Content/image/larry.jpg" },
                new Cat { Id = 2, Name = "Whisker", Breed = "Persian", Gender = "Male", Age = "2 years", Color = "Brown", Weight = "8 pounds", FavoriteToy = "Ball", FavoriteTreat = "Chicken", FavoriteActivity = "Climbing", ImageUrl = "~/Content/image/whisker.jpg" },
                new Cat { Id = 3, Name = "Milo", Breed = "Siamese", Gender = "Female", Age = "6 months", Color = "White", Weight = "5 pounds", FavoriteToy = "Feather", FavoriteTreat = "Milk", FavoriteActivity = "Running", ImageUrl = "~/Content/image/cat3.jpg" },
                new Cat { Id = 4, Name = "Oscar", Breed = "British Shorthair", Gender = "Male", Age = "3 years", Color = "Gray", Weight = "10 pounds", FavoriteToy = "Mouse", FavoriteTreat = "Fish", FavoriteActivity = "Sleeping", ImageUrl = "~/Content/image/cat4.jpg" }
            };
        } // [28]
    }
}
