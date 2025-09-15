using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class ShopController : Controller
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        private const string CartCookie = "CartId";

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

        private void EnsureCartTables()
        {
            var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Carts' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Carts (
        CartId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        UserId NVARCHAR(128) NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Carts_CreatedAt DEFAULT (GETDATE())
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CartItems' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.CartItems (
        CartId UNIQUEIDENTIFIER NOT NULL,
        ProductId INT NOT NULL,
        Quantity INT NOT NULL CHECK (Quantity >= 0),
        CONSTRAINT PK_CartItems PRIMARY KEY (CartId, ProductId),
        CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES dbo.Carts(CartId) ON DELETE CASCADE,
        CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END;";
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private void EnsureOrderTables()
        {
            var sql = @"
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Orders' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.Orders (
        OrderId INT IDENTITY(1,1) PRIMARY KEY,
        CartId UNIQUEIDENTIFIER NOT NULL,
        FullName NVARCHAR(200) NOT NULL,
        Email NVARCHAR(200) NULL,
        Phone NVARCHAR(50) NULL,
        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,
        PaymentMethod NVARCHAR(50) NULL,
        Notes NVARCHAR(1000) NULL,
        Subtotal DECIMAL(18,2) NOT NULL,
        Shipping DECIMAL(18,2) NOT NULL,
        Total DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (GETDATE())
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'OrderItems' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    CREATE TABLE dbo.OrderItems (
        OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        ProductId INT NOT NULL,
        Name NVARCHAR(100) NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL,
        CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
        CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES dbo.Products(Id)
    );
END;
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Orders_CartId')
    CREATE INDEX IX_Orders_CartId ON dbo.Orders(CartId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_OrderItems_OrderId')
    CREATE INDEX IX_OrderItems_OrderId ON dbo.OrderItems(OrderId);";
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

        private Guid GetOrCreateCartId()
        {
            var cookie = Request.Cookies[CartCookie];
            if (cookie != null && Guid.TryParse(cookie.Value, out var existing))
                return existing;

            var cartId = Guid.NewGuid();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("INSERT INTO dbo.Carts (CartId) VALUES (@CartId);", con))
            {
                cmd.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                con.Open();
                cmd.ExecuteNonQuery();
            }
            var hc = new HttpCookie(CartCookie, cartId.ToString())
            {
                HttpOnly = true,
                Secure = Request.IsSecureConnection,
                Expires = DateTime.UtcNow.AddDays(14),
                Path = "/"
            };
            Response.Cookies.Add(hc);
            return cartId;
        }

        private List<CartItem> GetCartItems(Guid cartId)
        {
            const string sql = @"
SELECT ci.ProductId, p.Name, p.Icon, p.Price AS UnitPrice, ci.Quantity
FROM dbo.CartItems ci
JOIN dbo.Products p ON p.Id = ci.ProductId
WHERE ci.CartId = @CartId
ORDER BY p.Name;";
            var list = new List<CartItem>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new CartItem
                        {
                            ProductId = r.GetInt32(0),
                            Name = r.GetString(1),
                            Icon = r.IsDBNull(2) ? null : r.GetString(2),
                            UnitPrice = r.GetDecimal(3),
                            Quantity = r.GetInt32(4)
                        });
                    }
                }
            }
            return list;
        }

        public ActionResult Index(string category)
        {
            EnsureProductsTable();
            EnsureCartTables();

            const string sql = @"
SELECT Id, Name, Description, Price, Stock, Category, Icon
FROM dbo.Products
WHERE (@Category IS NULL OR @Category = '' OR Category = @Category)
ORDER BY Id DESC;";
            var data = new List<Product>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Category", SqlDbType.NVarChar, 100).Value = (object)category ?? DBNull.Value;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                        data.Add(MapProduct(r));
                }
            }
            ViewBag.SelectedCategory = category;
            return View(data);
        }

        public ActionResult Details(int id)
        {
            EnsureProductsTable();
            EnsureCartTables();

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
        public ActionResult AddToCart(int productId, int quantity)
        {
            EnsureCartTables();
            var cartId = GetOrCreateCartId();

            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var update = new SqlCommand(
                        "UPDATE dbo.CartItems SET Quantity = Quantity + @Qty WHERE CartId = @CartId AND ProductId = @Pid;",
                        con, tx);
                    update.Parameters.Add("@Qty", SqlDbType.Int).Value = quantity;
                    update.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                    update.Parameters.Add("@Pid", SqlDbType.Int).Value = productId;
                    var rows = update.ExecuteNonQuery();

                    if (rows == 0)
                    {
                        var insert = new SqlCommand(
                            "INSERT INTO dbo.CartItems (CartId, ProductId, Quantity) VALUES (@CartId, @Pid, @Qty);",
                            con, tx);
                        insert.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                        insert.Parameters.Add("@Pid", SqlDbType.Int).Value = productId;
                        insert.Parameters.Add("@Qty", SqlDbType.Int).Value = quantity;
                        insert.ExecuteNonQuery();
                    }
                    tx.Commit();
                }
            }
            return RedirectToAction("Cart");
        }

        public ActionResult Cart()
        {
            EnsureCartTables();
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return View(new List<CartItem>());
            var items = GetCartItems(cartId);
            return View(items);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateCart(int productId, int quantity)
        {
            EnsureCartTables();
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return new HttpStatusCodeResult(400);

            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();
                if (quantity <= 0)
                {
                    var del = new SqlCommand("DELETE FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @Pid;", con);
                    del.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                    del.Parameters.Add("@Pid", SqlDbType.Int).Value = productId;
                    del.ExecuteNonQuery();
                }
                else
                {
                    var upd = new SqlCommand("UPDATE dbo.CartItems SET Quantity = @Qty WHERE CartId = @CartId AND ProductId = @Pid;", con);
                    upd.Parameters.Add("@Qty", SqlDbType.Int).Value = quantity;
                    upd.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                    upd.Parameters.Add("@Pid", SqlDbType.Int).Value = productId;
                    upd.ExecuteNonQuery();
                }
            }
            return new HttpStatusCodeResult(200);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveFromCart(int productId)
        {
            EnsureCartTables();
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return new HttpStatusCodeResult(400);

            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand("DELETE FROM dbo.CartItems WHERE CartId = @CartId AND ProductId = @Pid;", con))
            {
                cmd.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                cmd.Parameters.Add("@Pid", SqlDbType.Int).Value = productId;
                con.Open();
                cmd.ExecuteNonQuery();
            }
            return new HttpStatusCodeResult(200);
        }

        public ActionResult Checkout()
        {
            EnsureCartTables();
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return View(new List<CartItem>());
            return View(GetCartItems(cartId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ProcessOrder(FormCollection form)
        {
            EnsureOrderTables();
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return RedirectToAction("Cart");

            var items = GetCartItems(cartId);
            if (items == null || items.Count == 0)
                return RedirectToAction("Cart");

            decimal subtotal = items.Sum(i => i.UnitPrice * i.Quantity);
            decimal shipping = 60m;
            decimal total = subtotal + shipping;

            var fullName = form["fullName"];
            var email = form["email"];
            var phone = form["phone"];
            var address = form["address"];
            var city = form["city"];
            var postalCode = form["postalCode"];
            var paymentMethod = form["paymentMethod"];
            var notes = form["notes"];

            int newOrderId;
            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();
                using (var tx = con.BeginTransaction())
                {
                    var insertOrder = new SqlCommand(@"
INSERT INTO dbo.Orders
(CartId, FullName, Email, Phone, Address, City, PostalCode, PaymentMethod, Notes, Subtotal, Shipping, Total)
OUTPUT inserted.OrderId
VALUES (@CartId,@FullName,@Email,@Phone,@Address,@City,@PostalCode,@PaymentMethod,@Notes,@Subtotal,@Shipping,@Total);",
                        con, tx);

                    insertOrder.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                    insertOrder.Parameters.Add("@FullName", SqlDbType.NVarChar, 200).Value = (object)fullName ?? DBNull.Value;
                    insertOrder.Parameters.Add("@Email", SqlDbType.NVarChar, 200).Value = (object)email ?? DBNull.Value;
                    insertOrder.Parameters.Add("@Phone", SqlDbType.NVarChar, 50).Value = (object)phone ?? DBNull.Value;
                    insertOrder.Parameters.Add("@Address", SqlDbType.NVarChar, 500).Value = (object)address ?? DBNull.Value;
                    insertOrder.Parameters.Add("@City", SqlDbType.NVarChar, 100).Value = (object)city ?? DBNull.Value;
                    insertOrder.Parameters.Add("@PostalCode", SqlDbType.NVarChar, 20).Value = (object)postalCode ?? DBNull.Value;
                    insertOrder.Parameters.Add("@PaymentMethod", SqlDbType.NVarChar, 50).Value = (object)paymentMethod ?? DBNull.Value;
                    insertOrder.Parameters.Add("@Notes", SqlDbType.NVarChar, 1000).Value = (object)notes ?? DBNull.Value;

                    var pSub = insertOrder.Parameters.Add("@Subtotal", SqlDbType.Decimal); pSub.Precision = 18; pSub.Scale = 2; pSub.Value = subtotal;
                    var pShip = insertOrder.Parameters.Add("@Shipping", SqlDbType.Decimal); pShip.Precision = 18; pShip.Scale = 2; pShip.Value = shipping;
                    var pTot = insertOrder.Parameters.Add("@Total", SqlDbType.Decimal); pTot.Precision = 18; pTot.Scale = 2; pTot.Value = total;

                    newOrderId = (int)insertOrder.ExecuteScalar();

                    var insertItem = new SqlCommand(@"
INSERT INTO dbo.OrderItems (OrderId, ProductId, Name, UnitPrice, Quantity)
VALUES (@OrderId, @ProductId, @Name, @UnitPrice, @Quantity);", con, tx);

                    insertItem.Parameters.Add("@OrderId", SqlDbType.Int).Value = newOrderId;
                    var pPid = insertItem.Parameters.Add("@ProductId", SqlDbType.Int);
                    var pName = insertItem.Parameters.Add("@Name", SqlDbType.NVarChar, 100);
                    var pPrice = insertItem.Parameters.Add("@UnitPrice", SqlDbType.Decimal); pPrice.Precision = 18; pPrice.Scale = 2;
                    var pQty = insertItem.Parameters.Add("@Quantity", SqlDbType.Int);

                    foreach (var it in items)
                    {
                        pPid.Value = it.ProductId;
                        pName.Value = it.Name ?? (object)DBNull.Value;
                        pPrice.Value = it.UnitPrice;
                        pQty.Value = it.Quantity;
                        insertItem.ExecuteNonQuery();
                    }

                    var clear = new SqlCommand("DELETE FROM dbo.CartItems WHERE CartId = @CartId;", con, tx);
                    clear.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                    clear.ExecuteNonQuery();

                    tx.Commit();
                }
            }

            TempData["SuccessMessage"] = "Your order has been placed successfully!";
            return RedirectToAction("Index", "User");
        }


        public ActionResult Orders()
        {
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return View(Enumerable.Empty<OrderListItem>());

            var list = new List<OrderListItem>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(@"
SELECT OrderId, CreatedAt, FullName, Subtotal, Shipping, Total
FROM dbo.Orders
WHERE CartId = @CartId
ORDER BY OrderId DESC;", con))
            {
                cmd.Parameters.Add("@CartId", SqlDbType.UniqueIdentifier).Value = cartId;
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new OrderListItem
                        {
                            OrderId = r.GetInt32(0),
                            CreatedAt = r.GetDateTime(1),
                            FullName = r.IsDBNull(2) ? "" : r.GetString(2),
                            Subtotal = r.GetDecimal(3),
                            Shipping = r.GetDecimal(4),
                            Total = r.GetDecimal(5)
                        });
                    }
                }
            }
            return View(list);
        }


        public ActionResult OrderDetails(int id)
        {
            var cookie = Request.Cookies[CartCookie];
            if (cookie == null || !Guid.TryParse(cookie.Value, out var cartId))
                return HttpNotFound();

            // Load header
            int orderId = 0;
            DateTime createdAt = DateTime.MinValue;
            string fullName = "", email = "", phone = "", address = "", city = "", postalCode = "", paymentMethod = "";
            decimal subtotal = 0m, shipping = 0m, total = 0m;
            var items = new List<CartItem>();

            using (var con = new SqlConnection(ConnStr))
            {
                con.Open();

                using (var cmd = new SqlCommand(@"
SELECT TOP 1 OrderId, CartId, CreatedAt, FullName, Email, Phone, Address, City, PostalCode, PaymentMethod, Subtotal, Shipping, Total
FROM dbo.Orders WHERE OrderId = @OrderId;", con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            var orderCart = r.GetGuid(1);
                            if (orderCart != cartId) return HttpNotFound();

                            orderId = r.GetInt32(0);
                            createdAt = r.GetDateTime(2);
                            fullName = r.IsDBNull(3) ? "" : r.GetString(3);
                            email = r.IsDBNull(4) ? "" : r.GetString(4);
                            phone = r.IsDBNull(5) ? "" : r.GetString(5);
                            address = r.IsDBNull(6) ? "" : r.GetString(6);
                            city = r.IsDBNull(7) ? "" : r.GetString(7);
                            postalCode = r.IsDBNull(8) ? "" : r.GetString(8);
                            paymentMethod = r.IsDBNull(9) ? "" : r.GetString(9);
                            subtotal = r.GetDecimal(10);
                            shipping = r.GetDecimal(11);
                            total = r.GetDecimal(12);
                        }
                        else
                        {
                            return HttpNotFound();
                        }
                    }
                }

                // Load items
                using (var cmd = new SqlCommand(@"
SELECT ProductId, Name, UnitPrice, Quantity
FROM dbo.OrderItems
WHERE OrderId = @OrderId
ORDER BY OrderItemId;", con))
                {
                    cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = id;
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            items.Add(new CartItem
                            {
                                ProductId = r.GetInt32(0),
                                Name = r.GetString(1),
                                UnitPrice = r.GetDecimal(2),
                                Quantity = r.GetInt32(3)
                            });
                        }
                    }
                }
            }

            var vm = new Catzy.Models.OrderDetailsViewModel
            {
                OrderId = orderId,
                CreatedAt = createdAt,
                FullName = fullName,
                Address = address,
                City = city,
                PostalCode = postalCode,
                PaymentMethod = paymentMethod,
                Subtotal = subtotal,
                Shipping = shipping,
                Total = total,
                Items = items
            };

            return View(vm);
        }

    }
}
