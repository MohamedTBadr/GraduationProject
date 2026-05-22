using Domain.Contracts;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence
{
    public class DbIntialize(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole<Guid>> roleManager
    ) : IDbIntialize
    {
        public async Task IntializeAsync()
        {
            await context.Database.MigrateAsync();

            var strategy = context.Database.CreateExecutionStrategy(); // ← key fix

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await SeedVendorTypeAsync();
                    await SeedRolesAsync();
                    await SeedAdminUserAsync();
                    await SeedVendorUserAsync();
                    await SeedCustomerAsync();
                    await SeedServiceTypesAsync();
                    await SeedEventTypeAsync();
                    await SeedServicesAsync();
                    await SeedPackagesAsync();
                    await SeedNotificationAsnyc();
                    await SeedEventAsync();
                    await SeedOrderAsync();

                    await transaction.CommitAsync();

                    Console.WriteLine("Database seeding completed successfully.");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    Console.WriteLine("========================================");
                    Console.WriteLine("DATABASE SEEDING ERROR");
                    Console.WriteLine(ex);
                    Console.WriteLine("========================================");
                    throw; // let the strategy handle retries
                }
            });
        }

        private async Task SeedVendorTypeAsync()
        {
            if (await context.VendorTypes.AnyAsync())
                return;

            var vendorTypes = new List<VendorType>
            {
                new VendorType
                {
                    Id = Guid.NewGuid(),
                    Name = "Photographer"
                },
                new VendorType
                {
                    Id = Guid.NewGuid(),
                    Name = "Caterer"
                },
                new VendorType
                {
                    Id = Guid.NewGuid(),
                    Name = "Decorator"
                }
            };

            await context.VendorTypes.AddRangeAsync(vendorTypes);
            await context.SaveChangesAsync();
        }

        private async Task SeedEventTypeAsync()
        {
            if (!await context.EventTypes.AnyAsync())
            {
                var eventTypes = new List<EventType>
                {
                    new EventType
                    {
                        Id = Guid.NewGuid(),
                        Name = "Wedding"
                    },
                    new EventType
                    {
                        Id = Guid.NewGuid(),
                        Name = "Birthday"
                    },
                    new EventType
                    {
                        Id = Guid.NewGuid(),
                        Name = "Graduation"
                    }
                };

                await context.EventTypes.AddRangeAsync(eventTypes);
                await context.SaveChangesAsync();
            }
            else
            {
                var weeding = await context.EventTypes
                    .FirstOrDefaultAsync(e => e.Name == "Weeding");

                if (weeding != null)
                {
                    weeding.Name = "Wedding";
                    await context.SaveChangesAsync();
                }
            }
        }

        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Vendor", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(
                        new IdentityRole<Guid>
                        {
                            Id = Guid.NewGuid(),
                            Name = role,
                            NormalizedName = role.ToUpper()
                        });
                }
            }
        }

        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@example.com";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser != null)
                return;

            var newAdminUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                FirstName = "Mohamed",
                LastName = "Tarek",
                UserName = "admin",
                Email = adminEmail,
                NormalizedEmail = adminEmail.ToUpper(),
                NormalizedUserName = "ADMIN"
            };

            var result = await userManager.CreateAsync(
                newAdminUser,
                "Admin@123"
            );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            }
            else
            {
                Console.WriteLine(string.Join(",",
                    result.Errors.Select(e => e.Description)));
            }
        }

        private async Task SeedVendorUserAsync()
        {
            const string vendorEmail = "vendor@example.com";

            var vendorUser = await userManager.FindByEmailAsync(vendorEmail);

            if (vendorUser != null)
                return;

            var vendorType = await context.VendorTypes.FirstOrDefaultAsync();

            if (vendorType == null)
            {
                Console.WriteLine("VendorType not found.");
                return;
            }

            var newVendorUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "vendor",
                FirstName = "Mohamed",
                LastName = "Tarek",
                Email = vendorEmail,
                NormalizedEmail = vendorEmail.ToUpper(),
                NormalizedUserName = "VENDOR",
               
            };

            var result = await userManager.CreateAsync(
                newVendorUser,
                "Vendor@123"
            );

            if (!result.Succeeded)
            {
                Console.WriteLine(string.Join(",",
                    result.Errors.Select(e => e.Description)));

                return;
            }

            await userManager.AddToRoleAsync(newVendorUser, "Vendor");

            var vendorProfile = new Vendor
            {
                UserId = newVendorUser.Id,
                BusinessName = "Test Business",
                PortfolioLink = "https://example.com",
                Description = "Seeded Vendor",
                YearsInBusiness = 5,
                IsVerified = true,
                VendorTypeId = vendorType.Id,
                ProfilePicture = "https://example.com/profile.jpg",
                Document = "https://example.com/document.pdf",
                Address = new Address
                {
                    Street = "123 Test Street",
                    City = "Cairo",
                    State = "Cairo Governorate",
                }
                ,
                ServiceAreas = new List<ServiceArea>
                {
                   new ServiceArea
                  {
            Id = Guid.NewGuid(),
            City = "Cairo",
            Region = "Nasr City",
            Latitude = 30.0561m,
            Longitude = 31.3300m
        },

        new ServiceArea
        {
            Id = Guid.NewGuid(),
            City = "Cairo",
            Region = "Maadi",
            Latitude = 29.9602m,
            Longitude = 31.2569m
        },

        new ServiceArea
        {
            Id = Guid.NewGuid(),
            City = "Giza",
            Region = "Dokki",
            Latitude = 30.0384m,
            Longitude = 31.2122m
        }
           }
            };

            await context.Vendors.AddAsync(vendorProfile);
            await context.SaveChangesAsync();
        }

        private async Task SeedCustomerAsync()
        {
            const string customerEmail = "customer@example.com";

            var customerUser = await userManager.FindByEmailAsync(customerEmail);

            if (customerUser != null)
                return;

            var newCustomerUser = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "customer",
                FirstName = "Mohamed",
                LastName = "Tarek",
                Email = customerEmail,
                NormalizedEmail = customerEmail.ToUpper(),
                NormalizedUserName = "CUSTOMER",
                ReferralCode = "REF12345"
            };

            var result = await userManager.CreateAsync(
                newCustomerUser,
                "Customer@123"
            );

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(newCustomerUser, "Customer");
            }
            else
            {
                Console.WriteLine(string.Join(",",
                    result.Errors.Select(e => e.Description)));
            }
        }
      
        private async Task SeedServiceTypesAsync()
        {
            if (await context.ServiceTypes.AnyAsync())
                return;

            var photographer = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Photographer");

            var caterer = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Caterer");

            var decorator = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Decorator");

            if (photographer == null ||
                caterer == null ||
                decorator == null)
            {
                Console.WriteLine("VendorTypes missing.");
                return;
            }

            var serviceTypes = new List<ServiceType>
            {
                new ServiceType
                {
                    Id = Guid.NewGuid(),
                    Name = "Photography",
                    VendorTypeId = photographer.Id
                },
                new ServiceType
                {
                    Id = Guid.NewGuid(),
                    Name = "Catering",
                    VendorTypeId = caterer.Id
                },
                new ServiceType
                {
                    Id = Guid.NewGuid(),
                    Name = "Decoration",
                    VendorTypeId = decorator.Id
                }
            };

            await context.ServiceTypes.AddRangeAsync(serviceTypes);
            await context.SaveChangesAsync();
        }

        private async Task SeedServicesAsync()
        {
            if (await context.Services.AnyAsync())
                return;

            var vendor = await context.Vendors.FirstOrDefaultAsync();

            var photography = await context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Name == "Photography");

            var catering = await context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Name == "Catering");

            var decoration = await context.ServiceTypes
                .FirstOrDefaultAsync(s => s.Name == "Decoration");

            var wedding = await context.EventTypes
                .FirstOrDefaultAsync(e => e.Name == "Wedding");

            var birthday = await context.EventTypes
                .FirstOrDefaultAsync(e => e.Name == "Birthday");

            if (vendor == null ||
                photography == null ||
                catering == null ||
                decoration == null ||
                wedding == null ||
                birthday == null)
            {
                Console.WriteLine("Missing required data for service seeding.");
                return;
            }

            var services = new List<Service>
            {
                new Service
                {
                    Id = Guid.NewGuid(),
                    Name = "Wedding Photography Package",
                    Description = "Wedding photography coverage",
                    Price = 5000,
                    VendorId = vendor.UserId,
                    ServiceTypeId = photography.Id,
                    EventTypes = new List<EventType> { wedding }
                },
                new Service
                {
                    Id = Guid.NewGuid(),
                    Name = "Birthday Catering Set",
                    Description = "Birthday catering",
                    Price = 3000,
                    VendorId = vendor.UserId,
                    ServiceTypeId = catering.Id,
                    EventTypes = new List<EventType> { birthday }
                },
                new Service
                {
                    Id = Guid.NewGuid(),
                    Name = "Wedding Decoration",
                    Description = "Hall decoration",
                    Price = 7000,
                    VendorId = vendor.UserId,
                    ServiceTypeId = decoration.Id,
                    EventTypes = new List<EventType> { wedding }
                }
            };

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        private async Task SeedEventAsync()
        {
            // ── Guards ────────────────────────────────────────────────────────
            var user = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");
            var eventType = context.EventTypes.FirstOrDefault(x => x.Name == "Wedding");
            var vendor = context.ApplicationUsers.FirstOrDefault(v => v.Email == "vendor@example.com");

            if (user == null || eventType == null || vendor == null) return;

            // ── Lookup real Services (seeded before this step) ────────────────
            var cateringService = context.Services.FirstOrDefault(s => s.Name == "Premium Catering");
            var photoService = context.Services.FirstOrDefault(s => s.Name == "Wedding Photography Package");
            var decorService = context.Services.FirstOrDefault(s => s.Name == "Wedding Decoration");

            if (cateringService == null || photoService == null || decorService == null) return;

            // ── Build Event ───────────────────────────────────────────────────
            var newEvent = new Event
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                EventTypeId = eventType.Id,
                Title = "Luxury Wedding Cairo 2026",
                EventDate = DateTime.UtcNow.AddMonths(3),
                Location = new Address { City = "Cairo", State = "Giza", Street = "Pyramids Road" },
                TotalBudget = 75000m,
                GuestCount = 250,
                Notes = "Premium wedding with full vendor coordination",
                EventStatus = "Planned",

                // ── EventItems linked to real Services ────────────────────────
                EventItems = new List<EventItem>
        {
            new EventItem
            {
                Id             = Guid.NewGuid(),
                ServiceId      = cateringService.Id,
                Service        = cateringService,          // nav property — EF won't double-insert
                Price          = cateringService.Price,    // snapshot from actual service
                Quantity       = 1,
                ItemStatus     = "Approved",
                RejectionReason = null
            },
            new EventItem
            {
                Id        = Guid.NewGuid(),
                ServiceId = photoService.Id,
                Service   = photoService,
                Price     = photoService.Price,
                Quantity  = 1,
                ItemStatus = "Pending"
            },
            new EventItem
            {
                Id        = Guid.NewGuid(),
                ServiceId = decorService.Id,
                Service   = decorService,
                Price     = decorService.Price,
                Quantity  = 1,
                ItemStatus = "Pending"
            }
        }
            };

            // ── Skip if already seeded ────────────────────────────────────────
            var alreadySeeded = context.Events.Any(e => e.Title == newEvent.Title);
            if (alreadySeeded) return;

            await context.Events.AddAsync(newEvent);
            await context.SaveChangesAsync();
        }
        private async Task SeedPackagesAsync()
        {
            if (await context.Packages.AnyAsync())
                return;

            var vendor = await context.Vendors.FirstOrDefaultAsync();

            if (vendor == null)
            {
                Console.WriteLine("Vendor missing for package seeding.");
                return;
            }

            var packages = new List<Package>
            {
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Wedding Package",
                    Description = "Basic wedding package",
                    Price = 10000,
                    Discount = 10,
                    VendorId = vendor.UserId,
                    Items = new List<string>
                    {
                        "Photography",
                        "Decoration",
                        "Catering"
                    }
                },
                new Package
                {
                    Id = Guid.NewGuid(),
                    Name = "Premium Wedding Package",
                    Description = "Premium wedding package",
                    Price = 25000,
                    Discount = 15,
                    VendorId = vendor.UserId,
                    Items = new List<string>
                    {
                        "Luxury Photography",
                        "Luxury Decoration",
                        "Luxury Catering"
                    }
                }
            };

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }

        private async Task SeedNotificationAsnyc()
        {
                if (await context.Notifications.AnyAsync())
                    return;


            var vendor = context.ApplicationUsers
       .FirstOrDefault(u => u.Email == "vendor@example.com");
            var customer = context.ApplicationUsers
       .FirstOrDefault(u => u.Email == "customer@example.com");

            var notifications = new List<Notification>
{
    new Notification
    {
        Id = Guid.NewGuid(),
        UserId = vendor.Id, // Replace with actual user ID
        Message = "Welcome to our event management platform!",
        Title = "Test",
        Type = NotificationType.ACCOUNT_ACCEPTED,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    },

    new Notification
    {
        Id = Guid.NewGuid(),
        UserId = vendor.Id,
        Message = "Your vendor account has been approved.",
        Title = "Account Approved",
        Type = NotificationType.ACCOUNT_ACCEPTED,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    },

    new Notification
    {
        Id = Guid.NewGuid(),
        UserId = customer.Id,
        Message = "You received a new booking request.",
        Title = "New Booking",
        Type = NotificationType.ORDER_PLACED,
        IsRead = false,
        CreatedAt = DateTime.UtcNow
    }
};



            await context.Notifications.AddRangeAsync(notifications);
                await context.SaveChangesAsync();
        }

        private async Task SeedOrderAsync()
        {
            var user = context.ApplicationUsers
                .FirstOrDefault(u => u.Email == "customer@example.com");

            var existingEvent = context.Events
                .FirstOrDefault(e => e.Title == "Luxury Wedding Cairo 2026");

            if (user == null || existingEvent == null)
                return;

            var orderExists = context.Orders
                .Any(o => o.EventId == existingEvent.Id);

            if (orderExists)
                return;

            var order = new Order
            {
                Id = Guid.NewGuid(),

                UserId = user.Id,

                EventId = existingEvent.Id,

                Amount = 47000m,
                Currency = "EGP",

                PaymentIntentId = $"PAYMOB_{Guid.NewGuid():N}",
                PaymentStatus = "Pending",

                Appointment = DateTime.UtcNow.AddDays(10),

                ShippingAddress = new Address
                {
                    City = "Cairo",
                    State = "Giza",
                    Street = "Pyramids Road"
                },

                CreatedAt = DateTime.UtcNow
            };

            context.Orders.Add(order);

            await context.SaveChangesAsync();
        }
    }
}