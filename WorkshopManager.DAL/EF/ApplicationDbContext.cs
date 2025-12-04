using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.Model.DataModels;

namespace WorkshopManager.DAL.EF
{
    public class ApplicationDbContext : IdentityDbContext<User, Role, int>
    {
        public DbSet<RepairOrder> RepairOrders { get; set; }
        public DbSet<RepairTask> RepairTasks { get; set; }
        public DbSet<Mechanic> Mechanics { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            
            modelBuilder.Entity<User>().ToTable("AspNetUsers");
            modelBuilder.Entity<Role>().ToTable("AspNetRoles");

           
            modelBuilder.Entity<RepairOrder>()
                .Property(r => r.EntryEstimatedCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepairTask>()
                .Property(r => r.Cost)
                .HasPrecision(18, 2);
        }
    }
}