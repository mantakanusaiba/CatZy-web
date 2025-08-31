using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class ProductController : Controller
    {
        
        private static List<Product> products = new List<Product>
{
   
   
    new Product { Id=1, Name="Interactive Ball", Description="Motion-activated fun toy", Price=350, Stock=45, Category="Toys", Icon="ball.png" },
    new Product { Id=2, Name="Plush Mouse", Description="Soft toy mouse with catnip", Price=200, Stock=50, Category="Toys", Icon="mouse.png" },
    new Product { Id=3, Name="Vitamin", Description="Daily health support", Price=550, Stock=45, Category="Health Care", Icon="vitamin.png" },
    new Product { Id=4, Name="Shampoo", Description="Gentle shampoo for skin", Price=400, Stock=20, Category="Accessories", Icon="shampoo.png" },
    new Product { Id=5, Name="Flea Collar", Description="Protects from fleas and ticks", Price=600, Stock=35, Category="Accessories", Icon="collar.png" },
    new Product { Id=6, Name="Food Bowl", Description="Non-slip stainless steel", Price=500, Stock=45, Category="Pet Food", Icon="bowl.png" },
    new Product { Id=7, Name="Dry Cat Food", Description="Nutritious dry food ", Price=1200, Stock=60, Category="Pet Food", Icon="food.png" },
    new Product { Id=8, Name="Canned Food", Description="Tasty canned food ", Price=950, Stock=40, Category="Pet Food", Icon="can.png" }

};


        // GET: Product (All Products page)
        public ActionResult Index()
        {
            return View(products);
        }

        // GET: Product/Add (show form)
        public ActionResult Add()
        {
            return View();
        }

        // POST: Product/Add (save new product)
        [HttpPost]
        public ActionResult Add(Product model)
        {
            if (ModelState.IsValid)
            {
                model.Id = products.Count > 0 ? products.Max(p => p.Id) + 1 : 1;
                products.Add(model);
                return RedirectToAction("Index");
            }
            return View(model);
        }

        // GET: Product/Update/5
        public ActionResult Update(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // POST: Product/Update
        [HttpPost]
        public ActionResult Update(Product model)
        {
            var product = products.FirstOrDefault(p => p.Id == model.Id);
            if (product != null)
            {
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Stock = model.Stock;
                product.Category = model.Category;
                product.Icon = model.Icon;
            }
            return RedirectToAction("Index");
        }

        // GET: Product/Delete/5
        public ActionResult Delete(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                products.Remove(product);
            }
            return RedirectToAction("Index");
        }
        // GET: Product/Category/Toys
        public ActionResult Category(string category)
        {
            var filteredProducts = products
                .Where(p => p.Category == category)
                .ToList();

            ViewBag.SelectedCategory = category;
            return View("Index", filteredProducts); // Reuse Index view
        }

    }
}
