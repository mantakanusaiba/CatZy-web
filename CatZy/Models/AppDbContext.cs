using System.Collections.Generic;
using System.Data.Entity;

namespace Catzy.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext() : base("DefaultConnection") { }

        public DbSet<User> Users { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Appointment>().ToTable("Appointments"); 
            base.OnModelCreating(modelBuilder);
        }
    }
}
