using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.Mvc;
using Catzy.Models;

namespace Catzy.Controllers
{
    public class RescueController : Controller
    {
        private string ConnStr => ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        private void EnsureRescueRequestsTable()
        {
            var sql = @"
IF NOT EXISTS (
    SELECT 1
    FROM sys.tables t
    JOIN sys.schemas s ON s.schema_id = t.schema_id
    WHERE s.name = 'dbo' AND t.name = 'RescueRequests'
)
BEGIN
    CREATE TABLE dbo.RescueRequests (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        CatDescription NVARCHAR(255) NULL,
        LocationDescription NVARCHAR(255) NULL,
        Latitude FLOAT NOT NULL,
        Longitude FLOAT NOT NULL,
        CreatedAt DATETIME NOT NULL CONSTRAINT DF_RescueRequests_CreatedAt DEFAULT (GETDATE())
    );

    CREATE INDEX IX_RescueRequests_LatLng ON dbo.RescueRequests(Latitude, Longitude);
END;";
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(sql, con))
            {
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        [HttpGet]
        public ActionResult Report()
        {
            EnsureRescueRequestsTable();
           
            return View(new RescueRequest { Latitude = 23.8103, Longitude = 90.4125 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Report(RescueRequest model)
        {
            if (!ModelState.IsValid)
                return View(model);

            EnsureRescueRequestsTable();

            const string insertSql = @"
INSERT INTO dbo.RescueRequests (CatDescription, LocationDescription, Latitude, Longitude)
VALUES (@CatDescription, @LocationDescription, @Latitude, @Longitude);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int newId;
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(insertSql, con))
            {
              
                var p1 = cmd.Parameters.Add("@CatDescription", SqlDbType.NVarChar, 255);
                p1.Value = (object)model.CatDescription ?? DBNull.Value;

                var p2 = cmd.Parameters.Add("@LocationDescription", SqlDbType.NVarChar, 255);
                p2.Value = (object)model.LocationDescription ?? DBNull.Value;

                var p3 = cmd.Parameters.Add("@Latitude", SqlDbType.Float);
                p3.Value = model.Latitude;

                var p4 = cmd.Parameters.Add("@Longitude", SqlDbType.Float);
                p4.Value = model.Longitude;

                con.Open();
                newId = (int)cmd.ExecuteScalar();
            }

            TempData["SuccessMessage"] = "Rescue request submitted successfully! (ID: " + newId + ")";
            return RedirectToAction("Index", "User");
        }

        [HttpGet]
        public ActionResult AllReports()
        {
            EnsureRescueRequestsTable();

            const string selectSql = @"
SELECT Id, CatDescription, LocationDescription, Latitude, Longitude
FROM dbo.RescueRequests
ORDER BY Id DESC;";

            var list = new List<RescueRequest>();
            using (var con = new SqlConnection(ConnStr))
            using (var cmd = new SqlCommand(selectSql, con))
            {
                con.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        list.Add(new RescueRequest
                        {
                            Id = r.GetInt32(0),
                            CatDescription = r.IsDBNull(1) ? null : r.GetString(1),
                            LocationDescription = r.IsDBNull(2) ? null : r.GetString(2),
                            Latitude = r.GetDouble(3),
                            Longitude = r.GetDouble(4)
                        });
                    }
                }
            }

            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteRequest(int id)
        {

            const string sql = "DELETE FROM dbo.RescueRequests WHERE Id = @Id;";

            using (var con = new SqlConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString))
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id; 
                con.Open();
                cmd.ExecuteNonQuery();
            }

            TempData["SuccessMessage"] = "Rescue request deleted.";
            return RedirectToAction("AllReports");
        }

    }
}
