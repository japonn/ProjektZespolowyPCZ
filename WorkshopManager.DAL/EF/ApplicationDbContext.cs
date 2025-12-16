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
        public DbSet<AdditionalCost> AdditionalCosts { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Tabele Identity
            modelBuilder.Entity<User>()
                .ToTable("AspNetUsers")
                .HasDiscriminator<int>("UserType")
                .HasValue<User>((int)RoleValue.User)
                .HasValue<Owner>((int)RoleValue.Owner)
                .HasValue<Client>((int)RoleValue.Client);

            modelBuilder.Entity<Role>()
                .ToTable("AspNetRoles");

            // Precyzja dla wartości pieniężnych
            modelBuilder.Entity<RepairOrder>()
                .Property(r => r.EntryEstimatedCost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<RepairTask>()
                .Property(r => r.Cost)
                .HasPrecision(18, 2);

            modelBuilder.Entity<AdditionalCost>()
                .Property(a => a.Cost)
                .HasPrecision(18, 2);

            // Relacja RepairOrder -> AdditionalCosts
            modelBuilder.Entity<AdditionalCost>()
                .HasOne(a => a.RepairOrder)
                .WithMany(r => r.AdditionalCosts)
                .HasForeignKey(a => a.RepairOrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Seed podstawowych ról
            modelBuilder.Entity<Role>().HasData(
                new Role((int)RoleValue.User, nameof(RoleValue.User), RoleValue.User),
                new Role((int)RoleValue.Owner, nameof(RoleValue.Owner), RoleValue.Owner),
                new Role((int)RoleValue.Client, nameof(RoleValue.Client), RoleValue.Client)
            );
        }
    }
}
