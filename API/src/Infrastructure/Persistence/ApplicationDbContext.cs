using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;
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


           builder.Entity<OrderInsight>()
        .ToView("View_OrderInsights")
        .HasNoKey();
     


            builder.Entity<ServiceRating>()
                .HasOne(vr => vr.User)
                .WithMany()
                .HasForeignKey(vr => vr.UserId)
                .OnDelete(DeleteBehavior.NoAction); // 👈 Important

            builder.Entity<ServiceRating>()
                .HasOne(vr => vr.Service)
                .WithMany(s => s.ServiceRatings)
                .HasForeignKey(vr => vr.ServiceId)
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


            builder.Entity<Service>().HasMany(s=>s.EventTypes).WithMany(e=>e.Services)
                .UsingEntity(j => j.ToTable("ServiceEventTypes"));
        }


        public DbSet<OrderInsight> OrderInsights { get; set; }
        public DbSet<ServiceType> ServiceTypes { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<ServiceRating> ServiceRatings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventItem> EventItems { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<IdentityRole<Guid>> IdentityRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Package> Packages { get;set; }

        public DbSet<ServiceImage> ServiceImages { get; set; }

        public DbSet<VendorType> VendorTypes { get; set; }
        public DbSet<CorporationInquiry> CorporationInquiries { get; set; }


        public DbSet<EventType> EventTypes { get; set; } 

    }
}
