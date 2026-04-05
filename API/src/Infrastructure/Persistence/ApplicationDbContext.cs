using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
namespace Infrastructure.Persistence
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

            builder.Entity<Conversation>()
    .HasOne(c => c.User1)
    .WithMany()
    .HasForeignKey(c => c.User1Id)
    .OnDelete(DeleteBehavior.NoAction); // ✅ No cascade

            builder.Entity<Conversation>()
                .HasOne(c => c.User2)
                .WithMany()
                .HasForeignKey(c => c.User2Id)
                .OnDelete(DeleteBehavior.NoAction); // ✅ No cascade

            builder.Entity<Message>()
                .HasOne(m => m.Sender)
                .WithMany()
                .HasForeignKey(m => m.SenderId)
                .OnDelete(DeleteBehavior.NoAction); // ✅ No cascade

      
              builder.Entity<EventItem>()
                .HasOne(i => i.Event)
                .WithMany(e => e.EventItems)
                .HasForeignKey(i => i.EventId)
                .OnDelete(DeleteBehavior.NoAction); // ← fixes the cycle error
        }

        

        public DbSet<Category> Categories { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<VendorServiceType> VendorServiceTypes { get; set; }
        public DbSet<VendorRating> VendorRatings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventItem> EventItems { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<IdentityRole<Guid>> IdentityRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Package> Packages { get;set; }

        public DbSet<ServiceImage> ServiceImages { get; set; }

    }
}
