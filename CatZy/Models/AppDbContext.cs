using System.Collections.Generic;
using System.Data.Entity;

namespace Catzy.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }
    }
}
