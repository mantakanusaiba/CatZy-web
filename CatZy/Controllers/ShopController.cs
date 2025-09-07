using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class ShopController : Controller
    {
        private const string CartSession = "Cart";

        // Static product list (dummy data)
        private static List<Product> products = new List<Product>
        {
            new Product { Id=1, Name="Interactive Ball", Description="Motion-activated fun toy", Price=350, Stock=45, Category="Toys", Icon="ball.png" },
            new Product { Id=2, Name="Plush Mouse", Description="Soft toy mouse with catnip", Price=200, Stock=50, Category="Toys", Icon="mouse.png" },
            new Product { Id=3, Name="Vitamin", Description="Daily health support", Price=550, Stock=45, Category="Health Care", Icon="vitamin.png" },
            new Product { Id=4, Name="Shampoo", Description="Gentle shampoo for skin", Price=400, Stock=20, Category="Accessories", Icon="shampoo.png" },
            new Product { Id=5, Name="Flea Collar", Description="Protects from fleas and ticks", Price=600, Stock=35, Category="Accessories", Icon="collar.png" },
            new Product { Id=6, Name="Food Bowl", Description="Non-slip stainless steel", Price=500, Stock=45, Category="Pet Food", Icon="bowl.png" },
            new Product { Id=7, Name="Dry Cat Food", Description="Nutritious dry food ", Price=1200, Stock=60, Category="Pet Food", Icon="food.png" },
            new Product { Id=8, Name="Canned Food", Description="Tasty canned food ", Price=950, Stock=40, Category="Pet Food", Icon="can.png" },
            new Product { Id=9, Name="Pet Carrier", Description="Portable pet carrier ", Price=1550, Stock=20, Category="Accessories", Icon="carrier.png" }
        };

      
        private List<CartItem> GetCart()
        {
            if (Session[CartSession] == null)
                Session[CartSession] = new List<CartItem>();
            return (List<CartItem>)Session[CartSession];
        }

        //Shop Home
        public ActionResult Index(string category)
        {
            var data = string.IsNullOrEmpty(category)
                ? products
                : products.Where(p => p.Category == category).ToList();

            ViewBag.SelectedCategory = category;
            return View(data);
        }

        //Product Details
        public ActionResult Details(int id)
        {
            var product = products.FirstOrDefault(p => p.Id == id);
            if (product == null) return HttpNotFound();
            return View(product);
        }

        // Add to Cart
        [HttpPost]
        public ActionResult AddToCart(int productId, int quantity)
        {
            var product = products.FirstOrDefault(p => p.Id == productId);
            if (product == null) return HttpNotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(c => c.Product.Id == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem { Product = product, Quantity = quantity });

            Session[CartSession] = cart;
            return RedirectToAction("Cart");
        }

        // Show Cart
        public ActionResult Cart()
        {
            return View(GetCart());
        }

      
        [HttpPost]
        public ActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == productId);
            if (item != null)
            {
                item.Quantity = quantity; 
                if (item.Quantity <= 0) cart.Remove(item);
            }
            Session[CartSession] = cart;

            return new HttpStatusCodeResult(200);
        }


        [HttpPost]
        public ActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(c => c.Product.Id == productId);
            if (item != null) cart.Remove(item);
            Session[CartSession] = cart;

            return new HttpStatusCodeResult(200); // JS expects status, not redirect
        }

        // ✅ Checkout Page
        public ActionResult Checkout()
        {
            return View(GetCart());
        }


        [HttpPost]
        public ActionResult ProcessOrder(FormCollection form)
        {
            
            var fullName = form["fullName"];
            var email = form["email"];
            var phone = form["phone"];
            var address = form["address"];
            var city = form["city"];
            var postalCode = form["postalCode"];
            var paymentMethod = form["paymentMethod"];
            var notes = form["notes"];

            // Cart + totals
            var cartItems = form["cartItems"];
            var subtotal = form["subtotal"];
            var shipping = form["shipping"];
            var total = form["total"];

           
            Session["Cart"] = null;

           
            TempData["SuccessMessage"] = "Your order has been placed successfully!";

       
            return RedirectToAction("Index", "User");
        }

    }

}
