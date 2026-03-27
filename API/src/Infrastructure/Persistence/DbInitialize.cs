
using Domain.Contracts;
using Domain.Entities;
using Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace Infrastructure.Persistence
{
    public class DbIntialize(ApplicationDbContext context
        , UserManager<ApplicationUser> userManager
        , RoleManager<IdentityRole<Guid>> roleManager
) : IDbIntialize
    {
        public async Task IntializeAsync()
        {
            //Serviceion =>Seeding + Intialize Db
            if ((await context.Database.GetPendingMigrationsAsync()).Any())
            {
                await context.Database.MigrateAsync();
            }

            //Dev =>Seeding
            try
            {
                await SeedRolesAsync();
                await SeedAdminUserAsync();
                await SeedVendorUserAsync();
                await SeedCustomerAsync();
                await SeedCategoriesAsync();
                await SeedServiceTypesAsync();
                await SeedServicesAsync();   // ✅ add
                await SeedPackagesAsync();
            }
            catch (Exception E)
            {
                Console.WriteLine($"Error Occurred during seeding: {E.Message}");
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
            string adminEmail = "admin@example.com";
            if (userManager != null)
            {
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
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
                    var result = await userManager.CreateAsync(newAdminUser, "Admin@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newAdminUser, "Admin");
                    }
                }

            }
        }

        private async Task SeedVendorUserAsync()
        {
            string vendorEmail = "vendor@example.com";
            if (userManager != null)
            {
                var vendorUser = await userManager.FindByEmailAsync(vendorEmail);
                if (vendorUser == null)
                {
                    var newVendorUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = "vendor",
                        FirstName = "Mohamed",
                        LastName = "Tarek",
                        Email = vendorEmail,
                        NormalizedEmail = vendorEmail.ToUpper(),
                        NormalizedUserName = "VENDOR"
                    };
                    var vendorProfile = new Vendor
                    {
                        UserId = newVendorUser.Id,
                        BusinessName = "Test",
                        PortfolioLink = "...",
                        Description = "Test1",
                        IsVerified = true,

                    };
                    var result = await userManager.CreateAsync(newVendorUser, "Vendor@123");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newVendorUser, "Vendor");
                        await context.Vendors.AddAsync(vendorProfile);
                    }
                }
            }
        }
        private async Task SeedCustomerAsync()
        {
            string customerEmail = "customer@example.com";
            if (userManager != null)
            {
                var vendorUser = await userManager.FindByEmailAsync(customerEmail);
                if (vendorUser == null)
                {
                    var newVendorUser = new ApplicationUser
                    {
                        Id = Guid.NewGuid(),
                        UserName = "customer",
                        FirstName = "Mohamed",
                        LastName = "Tarek",
                        Email = customerEmail,
                        NormalizedEmail = customerEmail.ToUpper(),
                        NormalizedUserName = "CUSTOMER"
                    };
                    var result = await userManager.CreateAsync(newVendorUser, "Customer@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newVendorUser, "Customer");
                    }
                }
            }
        }
        private async Task SeedCategoriesAsync()
        {
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Id = Guid.NewGuid(), Name = "Weeding" },
                    new Category { Id = Guid.NewGuid(), Name = "Birthday" },
                    new Category { Id = Guid.NewGuid(), Name = "Graduation" },

                };
                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }
        }

        private async Task SeedServiceTypesAsync()
        {
            if (!context.ServiceTypes.Any())
            {
                var ServiceTypes = new List<ServiceType>
                {
                    new ServiceType { Id = Guid.NewGuid(), Name = "Photography" },
                    new ServiceType { Id = Guid.NewGuid(), Name = "Catering" },
                    new ServiceType { Id = Guid.NewGuid(), Name = "Decoration" },
                };
                context.ServiceTypes.AddRange(ServiceTypes);
                await context.SaveChangesAsync();
            }
        }
        private async Task SeedServicesAsync()
        {
            if (context.Services.Any()) return;

            // Pull existing seeded data to use as FKs
            var vendor = await context.Vendors.FirstOrDefaultAsync();
            var photography = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Photography");
            var catering = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Catering");
            var decoration = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Decoration");
            var wedding = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Weeding");
            var birthday = await context.Categories.FirstOrDefaultAsync(c => c.Name == "Birthday");

            if (vendor == null || photography == null || catering == null || wedding == null)
            {
                Console.WriteLine("Skipping Service seeding: required vendor/ServiceType/category not found.");
                return;
            }

            var Services = new List<Service>
    {
        new Service
        {
            Id = Guid.NewGuid(),
            Name = "Wedding Photography Package",
            Description = "Full-day wedding photography coverage with edited photos.",
            Price = 5000m,
            VendorId = vendor.UserId,
            ServiceTypeId = photography.Id,
            CategoryId = wedding.Id
        },
        new Service
        {
            Id = Guid.NewGuid(),
            Name = "Birthday Catering Set",
            Description = "Catering Service for up to 50 guests with custom menu.",
            Price = 3000m,
            VendorId = vendor.UserId,
            ServiceTypeId = catering.Id,
            CategoryId = birthday.Id
        },
        new Service
        {
            Id = Guid.NewGuid(),
            Name = "Wedding Hall Decoration",
            Description = "Full wedding hall decoration with flowers and lighting.",
            Price = 7000m,
            VendorId = vendor.UserId,
            ServiceTypeId = decoration.Id,
            CategoryId = wedding.Id
        }
    };

            await context.Services.AddRangeAsync(Services);
            await context.SaveChangesAsync();
        }

        private async Task SeedPackagesAsync()
        {
            if (context.Packages.Any()) return;

            var vendor = await context.Vendors.FirstOrDefaultAsync();

            if (vendor == null)
            {
                Console.WriteLine("Skipping package seeding: no vendor found.");
                return;
            }

            var packages = new List<Package>
    {
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Basic Wedding Package",
            Description = "Essential wedding Services bundle.",
            Price = 10000m,
            Discount = 10m,
            Items = new List<string>
            {
                "Wedding Photography Coverage",
                "Basic Hall Decoration",
                "Catering for 50 guests"
            },
            VendorId = vendor.UserId
        },
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Premium Wedding Package",
            Description = "All-inclusive luxury wedding experience.",
            Price = 25000m,
            Discount = 15m,
            Items = new List<string>
            {
                "Full-Day Photography & Videography",
                "Premium Floral Decoration",
                "Catering for 200 guests",
                "Live Music Band",
                "Luxury Car Rental"
            },
            VendorId = vendor.UserId
        },
        new Package
        {
            Id = Guid.NewGuid(),
            Name = "Birthday Starter Package",
            Description = "Fun birthday bundle for small gatherings.",
            Price = 4000m,
            Discount = 5m,
            Items = new List<string>
            {
                "Birthday Photography",
                "Balloon Decoration",
                "Catering for 30 guests"
            },
            VendorId = vendor.UserId
        }
    };

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }

    }
}