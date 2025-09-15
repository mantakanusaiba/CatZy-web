using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class ProductController : Controller
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private void EnsureProductsTable()
        {
            var sql = @"
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'dbo' AND t.name = 'Products'
)
BEGIN
    CREATE TABLE dbo.Products (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NULL,
        Price DECIMAL(18,2) NOT NULL,
        Stock INT NOT NULL,
        Category NVARCHAR(100) NOT NULL,
        Icon NVARCHAR(255) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT (GETDATE())
    );
    CREATE INDEX IX_Products_Category ON dbo.Products(Category);
END;";
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static Product MapProduct(IDataReader r)
        {
            return new Product
            {
                Id = r.GetInt32(0),
                Name = r.GetString(1),
                Description = r.IsDBNull(2) ? null : r.GetString(2),
                Price = r.GetDecimal(3),
                Stock = r.GetInt32(4),
                Category = r.GetString(5),
                Icon = r.IsDBNull(6) ? null : r.GetString(6)
            };
        }

        public ActionResult Index()
        {
            EnsureProductsTable();

            const string sql = @"
SELECT Id, Name, Description, Price, Stock, Category, Icon
FROM dbo.Products
ORDER BY Id DESC;";

            var list = new List<Product>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapProduct(r));
                }
            }

            return View(list);
        }

        [HttpGet]
        public ActionResult Add()
        {
            EnsureProductsTable();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Product model)
        {
            if (!ModelState.IsValid)
                return View(model);

            EnsureProductsTable();

            const string sql = @"
INSERT INTO dbo.Products (Name, Description, Price, Stock, Category, Icon)
VALUES (@Name, @Description, @Price, @Stock, @Category, @Icon);";

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = model.Name;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object)model.Description ?? DBNull.Value;

                var pPrice = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
                pPrice.Precision = 18;  // DECIMAL(18,2)
                pPrice.Scale = 2;
                pPrice.Value = model.Price;

                cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = model.Stock;
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = model.Category;
                cmd.Parameters.Add("@Icon", SqlDbType.NVarChar, 255).Value = (object)model.Icon ?? DBNull.Value;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult Update(int id)
        {
            EnsureProductsTable();

            const string sql = @"
SELECT Id, Name, Description, Price, Stock, Category, Icon
FROM dbo.Products
WHERE Id = @Id;";

            Product product = null;
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    if (r.Read())
                        product = MapProduct(r);
                }
            }

            if (product == null) return HttpNotFound();
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(Product model)
        {
            if (!ModelState.IsValid)
                return View(model);

            EnsureProductsTable();

            const string sql = @"
UPDATE dbo.Products
SET Name = @Name,
    Description = @Description,
    Price = @Price,
    Stock = @Stock,
    Category = @Category,
    Icon = @Icon
WHERE Id = @Id;";

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Name", SqlDbType.NVarChar, 100).Value = model.Name;
                cmd.Parameters.Add("@Description", SqlDbType.NVarChar, 500).Value = (object)model.Description ?? DBNull.Value;

                var pPrice = cmd.Parameters.Add("@Price", SqlDbType.Decimal);
                pPrice.Precision = 18;
                pPrice.Scale = 2;
                pPrice.Value = model.Price;

                cmd.Parameters.Add("@Stock", SqlDbType.Int).Value = model.Stock;
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = model.Category;
                cmd.Parameters.Add("@Icon", SqlDbType.NVarChar, 255).Value = (object)model.Icon ?? DBNull.Value;
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = model.Id;

                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            EnsureProductsTable();

            const string sql = "DELETE FROM dbo.Products WHERE Id = @Id;";

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                con.Open();
                cmd.ExecuteNonQuery();
            }

            return RedirectToAction("Index");
        }

        public ActionResult Category(string category)
        {
            EnsureProductsTable();

            const string sql = @"
SELECT Id, Name, Description, Price, Stock, Category, Icon
FROM dbo.Products
WHERE Category = @Category
ORDER BY Name;";

            var list = new List<Product>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = category ?? string.Empty;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        list.Add(MapProduct(r));
                }
            }

            ViewBag.SelectedCategory = category;
            return View("Index", list);
        }
    }
}
