using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HRMS.Models;
using HRMS.Areas.IdentityManager.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;

namespace HRMS.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<ApplicationUser>().HasMany(p => p.Roles).WithOne().HasForeignKey(p => p.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<ApplicationUser>().HasMany(e => e.Claims).WithOne().HasForeignKey(e => e.UserId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<ApplicationRole>().HasMany(r => r.Claims).WithOne().HasForeignKey(r => r.RoleId).IsRequired().OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Transaction>().HasOne(t => t.Room).WithMany().HasForeignKey(t => t.RoomId).OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<HRMS.Models.Agent> Agent { get; set; }
        public DbSet<HRMS.Models.Service> Service { get; set; }
        public DbSet<HRMS.Models.Guest> Guest { get; set; }
        public DbSet<HRMS.Models.Restaurant> Restaurant { get; set; }
        public DbSet<HRMS.Models.Room> Room { get; set; }
        public DbSet<HRMS.Models.RoomType> RoomType { get; set; }
        public DbSet<HRMS.Models.Transaction> Transaction { get; set; }
        public DbSet<HRMS.Models.Booking> Booking { get; set; }
        public DbSet<HRMS.Models.CheckedInOutTime> CheckedInOutTime { get; set; }
        public DbSet<HRMS.Models.CheckedInCustomer> CheckedInCustomer { get; set; }
        public DbSet<HRMS.Models.Receipt> Receipt { get; set; }



    }
}