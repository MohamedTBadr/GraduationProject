using DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // 🔥 REQUIRED

            builder.Entity<Vendor>()
                .HasKey(v => v.UserId);

            builder.Entity<Vendor>()
                .HasOne(v => v.User)
                .WithOne()
                .HasForeignKey<Vendor>(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.Entity<VendorServiceType>()
      .HasKey(vs => new { vs.VendorId, vs.ServiceTypeId });

            builder.Entity<VendorServiceType>()
                .HasOne(vs => vs.Vendor)
                .WithMany(v => v.VendorServiceTypes)
                .HasForeignKey(vs => vs.VendorId);

            builder.Entity<VendorServiceType>()
                .HasOne(vs => vs.ServiceType)
                .WithMany(st => st.VendorServiceTypes)
                .HasForeignKey(vs => vs.ServiceTypeId);



            builder.Entity<VendorRating>()
                .HasOne(vr => vr.User)
                .WithMany()
                .HasForeignKey(vr => vr.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 Important

            builder.Entity<VendorRating>()
                .HasOne(vr => vr.Vendor)
                .WithMany(v => v.VendorRatings)
                .HasForeignKey(vr => vr.VendorId)
                .OnDelete(DeleteBehavior.Cascade); // 👈 keep this
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
         public DbSet<OrderItem> OrderItems { get; set; }
            public DbSet<Vendor> Vendors { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<IdentityRole<Guid>> IdentityRoles { get; set; }


    }
}
