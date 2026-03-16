
using Domain.Contracts;
using Domain.Entities;
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
            //production =>Seeding + Intialize Db
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
                await SeedCategoriesAsync();
                await SeedServiceTypesAsync();

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
                        Email = vendorEmail,
                        NormalizedEmail = vendorEmail.ToUpper(),
                        NormalizedUserName = "VENDOR"
                    };
                    var result = await userManager.CreateAsync(newVendorUser, "Vendor@123");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(newVendorUser, "Vendor");
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
                var serviceTypes = new List<ServiceType>
                {
                    new ServiceType { Id = Guid.NewGuid(), Name = "Photography" },
                    new ServiceType { Id = Guid.NewGuid(), Name = "Catering" },
                    new ServiceType { Id = Guid.NewGuid(), Name = "Decoration" },
                };
                context.ServiceTypes.AddRange(serviceTypes);
                await context.SaveChangesAsync();
            }
        }
    }
}