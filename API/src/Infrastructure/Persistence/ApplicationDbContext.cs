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

            builder.Entity<Vendor>(entity =>
            {
                entity.HasKey(v => v.UserId);

                entity.Property(v => v.BusinessName)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(v => v.Description)
                      .HasMaxLength(1000);

                entity.Property(v => v.PortfolioLink)
                      .HasMaxLength(500);

                entity.Property(v => v.YearsInBusiness)
                      .HasColumnType("decimal(5,1)");

                entity.Property(v => v.IsVerified)
                      .HasDefaultValue(false);

                entity.OwnsOne(v => v.Address, address =>
                {
                    address.Property(a => a.Street).HasMaxLength(255).IsRequired(false);
                    address.Property(a => a.City).HasMaxLength(100).IsRequired(false);
                    address.Property(a => a.State).HasMaxLength(100).IsRequired(false);
                });

                entity.HasOne(v => v.User)
                      .WithOne()
                      .HasForeignKey<Vendor>(v => v.UserId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(v => v.VendorType)
                      .WithMany(vt => vt.Vendors)
                      .HasForeignKey(v => v.VendorTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(v => v.Services)
                      .WithOne(s => s.Vendor)
                      .HasForeignKey(s => s.VendorId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(v => v.Packages)
                      .WithOne(p => p.Vendor)
                      .HasForeignKey(p => p.VendorId)
                      .OnDelete(DeleteBehavior.Cascade);

            
            });

            builder.Entity<OrderInsight>()
        .ToView("View_OrderInsights")
        .HasNoKey();

            builder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.Property(o => o.Amount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(o => o.Currency)
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(o => o.PaymentIntentId)
                      .HasMaxLength(255);

                entity.Property(o => o.PaymentStatus)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.OwnsOne(o => o.ShippingAddress, address =>
                {
                    address.Property(a => a.Street).HasMaxLength(255);
                    address.Property(a => a.City).HasMaxLength(100);
                    address.Property(a => a.State).HasMaxLength(100);
                });

                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(o => o.Event)
                      .WithOne(e => e.Order)
                      .HasForeignKey<Order>(o => o.EventId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(o => o.EventId); // remove .IsUnique() since multiple orders per event is valid
            });
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


            builder.Entity<Event>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.Property(e => e.Title)
                      .HasMaxLength(255)
                      .IsRequired();

                entity.Property(e => e.EventStatus)
                      .HasMaxLength(50)
                      .IsRequired();

                entity.Property(e => e.TotalBudget)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired();

                entity.Property(e => e.GuestCount)
                      .IsRequired();

                entity.Property(e => e.Notes)
                      .HasMaxLength(1000);

                entity.Property(e => e.AdditionalNotes)
                      .HasMaxLength(1000);

                entity.Property(e => e.CancellationReason)
                      .HasMaxLength(500);

                entity.OwnsOne(e => e.Location, address =>
                {
                    address.Property(a => a.Street).HasMaxLength(255);
                    address.Property(a => a.City).HasMaxLength(100);
                    address.Property(a => a.State).HasMaxLength(100);
                });

                entity.HasOne(e => e.User)
                      .WithMany(u => u.Events)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.EventType)
                      .WithMany(et => et.Events)
                      .HasForeignKey(e => e.EventTypeId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(e => e.EventItems)
                      .WithOne(ei => ei.Event)
                      .HasForeignKey(ei => ei.EventId)
                      .OnDelete(DeleteBehavior.Cascade); // deleting event removes its items

                entity.HasOne(e => e.Order)
                      .WithOne(o => o.Event)
                      .HasForeignKey<Order>(o => o.EventId)
                      .OnDelete(DeleteBehavior.Restrict);
            }); builder.Entity<EventItem>()
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
        public DbSet<Vendor> Vendors { get; set; }
        public DbSet<ServiceRating> ServiceRatings { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<EventItem> EventItems { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<IdentityRole<Guid>> IdentityRoles { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }
        public DbSet<SupportAgent> SupportAgents { get; set; }
        public DbSet<TicketReply> TicketReplies { get; set; }

        public DbSet<Notification> Notifications { get; set; }

        public DbSet<Package> Packages { get;set; }

        public DbSet<ServiceImage> ServiceImages { get; set; }

        public DbSet<VendorType> VendorTypes { get; set; }
        public DbSet<CorporationInquiry> CorporationInquiries { get; set; }


        public DbSet<EventType> EventTypes { get; set; } 

    }
}
