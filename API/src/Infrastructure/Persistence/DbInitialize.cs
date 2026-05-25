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

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await SeedVendorTypeAsync();
                    await SeedRolesAsync();
                    await SeedAdminUserAsync();
                    await SeedVendorUserAsync();
                    await SeedVenueVendorsAsync();
                    await SeedCoworkingSpaceVendorsAsync();
                    await SeedProductionVendorsAsync();
                    await SeedCateringVendorsAsync();
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
                    throw;
                }
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        //  VENDOR TYPES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedVendorTypeAsync()
        {
            if (await context.VendorTypes.AnyAsync())
                return;

            var vendorTypes = new List<VendorType>
            {
                new VendorType { Id = Guid.NewGuid(), Name = "Photographer"     },
                new VendorType { Id = Guid.NewGuid(), Name = "Caterer"          },
                new VendorType { Id = Guid.NewGuid(), Name = "Decorator"        },
                new VendorType { Id = Guid.NewGuid(), Name = "Venue"            },
                new VendorType { Id = Guid.NewGuid(), Name = "Coworking Space"  },
                new VendorType { Id = Guid.NewGuid(), Name = "Production"       }
            };

            await context.VendorTypes.AddRangeAsync(vendorTypes);
            await context.SaveChangesAsync();
        }


        // ─────────────────────────────────────────────────────────────────────
        //  VENUE VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedVenueVendorsAsync()
        {
            var venueEmails = new[]
            {
        "info@swissclubcairo.com",
        "auctahrirsquare@aucegypt.edu",
        "info@mqrspaces.com",
        "nessma.mamdouh@wrk.plus",
        "info@elsawyculturewheel.com",
        "mhrs.caimn.ebc@marriott.com",
        "h5307@sofitel.com",
        "venue.cairovenue@placeholder.com",
        "venue.darb1718@placeholder.com",
        "venue.townhouse@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => venueEmails.Contains(u.Email)))
                return;

            var venueType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Venue");

            if (venueType == null)
            {
                Console.WriteLine("[VenueSeeding] VendorType 'Venue' not found.");
                return;
            }

            var venueData = new[]
            {
        new
        {
            BusinessName = "Swiss Club Cairo",
            Phone = "01003009695",
            Email = "info@swissclubcairo.com",
            Street = "Kit Kat",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Kit Kat", 30.0705m, 31.2137m)
            }
        },

        new
        {
            BusinessName = "AUC Tahrir Culture Centre",
            Phone = "0226151000",
            Email = "auctahrirsquare@aucegypt.edu",
            Street = "113 Kasr El Ainy",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        },

        new
        {
            BusinessName = "MQR Spaces",
            Phone = "01156868648",
            Email = "info@mqrspaces.com",
            Street = "Downtown Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m),
                ("Cairo", "Maadi", 29.9602m, 31.2569m),
                ("Cairo", "New Cairo", 30.0120m, 31.4354m),
                ("Giza", "6th October", 29.9285m, 30.9188m)
            }
        },

        new
        {
            BusinessName = "The Greek Campus",
            Phone = "01033350056",
            Email = "nessma.mamdouh@wrk.plus",
            Street = "Falaki Square",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        },

        new
        {
            BusinessName = "El Sawy Culturewheel",
            Phone = "0227354448",
            Email = "info@elsawyculturewheel.com",
            Street = "26th July Corridor, Zamalek",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Marriott Mena House Cairo",
            Phone = "0233773222",
            Email = "mhrs.caimn.ebc@marriott.com",
            Street = "6 Pyramids Road",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Haram", 29.9773m, 31.1325m)
            }
        },

        new
        {
            BusinessName = "Sofitel Cairo Nile El Gezirah",
            Phone = "0227373737",
            Email = "h5307@sofitel.com",
            Street = "3 El Thawra Council St",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0555m, 31.2243m)
            }
        },

        new
        {
            BusinessName = "Cairo Venue",
            Phone = "01211140222",
            Email = "venue.cairovenue@placeholder.com",
            Street = "Zamalek",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Darb 1718",
            Phone = "",
            Email = "venue.darb1718@placeholder.com",
            Street = "Fustat",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Old Cairo", 30.0062m, 31.2315m)
            }
        },

        new
        {
            BusinessName = "Townhouse Gallery",
            Phone = "",
            Email = "venue.townhouse@placeholder.com",
            Street = "Downtown Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        }
    };

            await SeedVendorListAsync(
                venueData.Select(v =>
                    (
                        v.BusinessName,
                        v.Phone,
                        v.Email,
                        v.Street,
                        v.City,
                        v.State,
                        v.Regions
                    )),
                venueType,
                "VenueSeeding");

            Console.WriteLine("[VenueSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  COWORKING SPACE VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedCoworkingSpaceVendorsAsync()
        {
            var coworkingEmails = new[]
            {
        "hello@consoleya.com",
        "admin@cubespaceeg.com",
        "info@kmthouse.com",
        "hello@thedistrict-eg.com",
        "info@almaqarr.com",
        "vlab@aucegypt.edu",
        "coworking.regus@placeholder.com",
        "coworking.startuphaus@placeholder.com",
        "coworking.villam@placeholder.com",
        "coworking.garage@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => coworkingEmails.Contains(u.Email)))
                return;

            var coworkingType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Coworking Space");

            if (coworkingType == null)
            {
                Console.WriteLine("[CoworkingSeeding] VendorType 'Coworking Space' not found.");
                return;
            }

            var coworkingData = new[]
            {
        new
        {
            BusinessName = "Consoleya",
            Phone = "01200026821",
            Email = "hello@consoleya.com",
            Street = "5 El-Fadl Street",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        },

        new
        {
            BusinessName = "Cube Space",
            Phone = "01050099559",
            Email = "admin@cubespaceeg.com",
            Street = "Nasr City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Nasr City", 30.0561m, 31.3300m)
            }
        },

        new
        {
            BusinessName = "KMT House",
            Phone = "01099997858",
            Email = "info@kmthouse.com",
            Street = "Garden City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Garden City", 30.0393m, 31.2280m)
            }
        },

        new
        {
            BusinessName = "The District",
            Phone = "01119911147",
            Email = "hello@thedistrict-eg.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "AlMaqarr",
            Phone = "01114447093",
            Email = "info@almaqarr.com",
            Street = "Sheikh Zayed",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m)
            }
        },

        new
        {
            BusinessName = "AUC Venture Lab",
            Phone = "0226154000",
            Email = "vlab@aucegypt.edu",
            Street = "AUC New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Regus Egypt",
            Phone = "0227586500",
            Email = "coworking.regus@placeholder.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Startup Haus Cairo",
            Phone = "",
            Email = "coworking.startuphaus@placeholder.com",
            Street = "Downtown Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        },

        new
        {
            BusinessName = "Villa M by Marakez",
            Phone = "",
            Email = "coworking.villam@placeholder.com",
            Street = "Zamalek",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Garage El Mahrousa",
            Phone = "01026030075",
            Email = "coworking.garage@placeholder.com",
            Street = "Bab El Louk",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Abdeen", 30.0450m, 31.2450m)
            }
        }
    };

            await SeedVendorListAsync(
                coworkingData.Select(v =>
                    (
                        v.BusinessName,
                        v.Phone,
                        v.Email,
                        v.Street,
                        v.City,
                        v.State,
                        v.Regions
                    )),
                coworkingType,
                "CoworkingSeeding");

            Console.WriteLine("[CoworkingSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRODUCTION VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedProductionVendorsAsync()
        {
            var productionEmails = new[]
            {
        "info@merakiandbeyond.com",
        "info@eventplaneteg.com",
        "clientservice@leapfrog.com.eg",
        "info@septemevents.com",
        "info@promolinks-int.com",
        "info@vision-events.net",
        "production.eventhouse@placeholder.com",
        "production.creativeexpo@placeholder.com",
        "production.gitex@placeholder.com",
        "production.eventec@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => productionEmails.Contains(u.Email)))
                return;

            var productionType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Production");

            if (productionType == null)
            {
                Console.WriteLine("[ProductionSeeding] VendorType 'Production' not found.");
                return;
            }

            var productionData = new[]
            {
        new
        {
            BusinessName = "Meraki and Beyond",
            Phone = "01008887463",
            Email = "info@merakiandbeyond.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Event Planet",
            Phone = "01003389330",
            Email = "info@eventplaneteg.com",
            Street = "Nasr City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Nasr City", 30.0561m, 31.3300m)
            }
        },

        new
        {
            BusinessName = "LEAPFROG",
            Phone = "01222127010",
            Email = "clientservice@leapfrog.com.eg",
            Street = "Heliopolis",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Heliopolis", 30.0876m, 31.3220m)
            }
        },

        new
        {
            BusinessName = "Septem Event Services",
            Phone = "01002220091",
            Email = "info@septemevents.com",
            Street = "Sheikh Zayed",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m)
            }
        },

        new
        {
            BusinessName = "PromoLinks",
            Phone = "0225190393",
            Email = "info@promolinks-int.com",
            Street = "Mohandessin",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Mohandessin", 30.0495m, 31.1990m)
            }
        },

        new
        {
            BusinessName = "Vision Events",
            Phone = "01005151518",
            Email = "info@vision-events.net",
            Street = "Maadi",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Maadi", 29.9602m, 31.2569m)
            }
        },

        new
        {
            BusinessName = "Event House Egypt",
            Phone = "0226905171",
            Email = "production.eventhouse@placeholder.com",
            Street = "Heliopolis",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Heliopolis", 30.0876m, 31.3220m)
            }
        },

        new
        {
            BusinessName = "Creative Expo",
            Phone = "01090009301",
            Email = "production.creativeexpo@placeholder.com",
            Street = "Nasr City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Nasr City", 30.0561m, 31.3300m)
            }
        },

        new
        {
            BusinessName = "Gitex Events",
            Phone = "01151853242",
            Email = "production.gitex@placeholder.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Eventec",
            Phone = "01284977748",
            Email = "production.eventec@placeholder.com",
            Street = "Downtown Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Downtown", 30.0444m, 31.2357m)
            }
        }
    };

            await SeedVendorListAsync(
                productionData.Select(v =>
                    (
                        v.BusinessName,
                        v.Phone,
                        v.Email,
                        v.Street,
                        v.City,
                        v.State,
                        v.Regions
                    )),
                productionType,
                "ProductionSeeding");

            Console.WriteLine("[ProductionSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CATERING VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedCateringVendorsAsync()
        {
            var cateringEmails = new[]
            {
        "catering.abouelsid@placeholder.com",
        "catering.zooba@placeholder.com",
        "catering.tabali@placeholder.com",
        "catering.etoile@placeholder.com",
        "catering.cilantro@placeholder.com",
        "catering.sequoia@placeholder.com",
        "catering.kazouza@placeholder.com",
        "catering.cookdoor@placeholder.com",
        "catering.didos@placeholder.com",
        "catering.willyskitchen@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => cateringEmails.Contains(u.Email)))
                return;

            var catererType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Caterer");

            if (catererType == null)
            {
                Console.WriteLine("[CateringSeeding] VendorType 'Caterer' not found.");
                return;
            }

            var cateringData = new[]
            {
        new
        {
            BusinessName = "Abou El Sid Catering",
            Phone = "0227359640",
            Email = "catering.abouelsid@placeholder.com",
            Street = "Zamalek",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Zooba Catering",
            Phone = "01222220656",
            Email = "catering.zooba@placeholder.com",
            Street = "Zamalek",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Tabali Catering",
            Phone = "01010959557",
            Email = "catering.tabali@placeholder.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Etoile Catering",
            Phone = "01116336313",
            Email = "catering.etoile@placeholder.com",
            Street = "Heliopolis",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Heliopolis", 30.0876m, 31.3220m)
            }
        },

        new
        {
            BusinessName = "Cilantro Catering",
            Phone = "0227514040",
            Email = "catering.cilantro@placeholder.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
            }
        },

        new
        {
            BusinessName = "Sequoia Events Catering",
            Phone = "01270005551",
            Email = "catering.sequoia@placeholder.com",
            Street = "Zamalek",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Zamalek", 30.0626m, 31.2197m)
            }
        },

        new
        {
            BusinessName = "Kazouza Catering",
            Phone = "01004411993",
            Email = "catering.kazouza@placeholder.com",
            Street = "Sheikh Zayed",
            City = "Giza",
            State = "Giza Governorate",
            Regions = new[]
            {
                ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m)
            }
        },

        new
        {
            BusinessName = "Cook Door Catering",
            Phone = "16999",
            Email = "catering.cookdoor@placeholder.com",
            Street = "Nasr City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Nasr City", 30.0561m, 31.3300m)
            }
        },

        new
        {
            BusinessName = "Dido's Catering",
            Phone = "01222241041",
            Email = "catering.didos@placeholder.com",
            Street = "Maadi",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Maadi", 29.9602m, 31.2569m)
            }
        },

        new
        {
            BusinessName = "Willy's Kitchen Catering",
            Phone = "01004040475",
            Email = "catering.willyskitchen@placeholder.com",
            Street = "Heliopolis",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Heliopolis", 30.0876m, 31.3220m)
            }
        }
    };

            await SeedVendorListAsync(
                cateringData.Select(v =>
                    (
                        v.BusinessName,
                        v.Phone,
                        v.Email,
                        v.Street,
                        v.City,
                        v.State,
                        v.Regions
                    )),
                catererType,
                "CateringSeeding");

            Console.WriteLine("[CateringSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SHARED HELPER — creates ApplicationUser + Vendor + ServiceAreas
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedVendorListAsync(
            IEnumerable<(string BusinessName, string Phone, string Email, string Street, string City, string State, (string, string, decimal, decimal)[] Regions)> vendors,
            VendorType vendorType,
            string logPrefix)
        {
            foreach (var v in vendors)
            {
                if (await context.ApplicationUsers.AnyAsync(u => u.Email == v.Email))
                    continue;

                var appUser = new ApplicationUser
                {
                    Id                 = Guid.NewGuid(),
                    UserName           = v.Email,
                    FirstName          = v.BusinessName,
                    LastName           = vendorType.Name,
                    Email              = v.Email,
                    NormalizedEmail    = v.Email.ToUpper(),
                    NormalizedUserName = v.Email.ToUpper(),
                    PhoneNumber        = v.Phone,
                   
                };

                var result = await userManager.CreateAsync(appUser, "Vendor@123");

                if (!result.Succeeded)
                {
                    Console.WriteLine($"[{logPrefix}] Failed to create {v.Email}: " +
                                      string.Join(", ", result.Errors.Select(e => e.Description)));
                    continue;
                }

                await userManager.AddToRoleAsync(appUser, "Vendor");

                var serviceAreas = v.Regions
                    .Select(r => new ServiceArea
                    {
                        Id        = Guid.NewGuid(),
                        City      = r.Item1,
                        Region    = r.Item2,
                        Latitude  = r.Item3,
                        Longitude = r.Item4
                    })
                    .ToList();

                var vendorProfile = new Vendor
                {
                    UserId          = appUser.Id,
                    BusinessName    = v.BusinessName,
                    PortfolioLink   = string.Empty,
                    Description     = $"{v.BusinessName} – {vendorType.Name} provider.",
                    YearsInBusiness = 0,
                    IsVerified      = true,
                    VendorTypeId    = vendorType.Id,
                    ProfilePicture  = string.Empty,
                    Document        = string.Empty,
                    Address = new Address
                    {
                        Street = v.Street,
                        City   = v.City,
                        State  = v.State
                    },
                    ServiceAreas = serviceAreas
                };

                await context.Vendors.AddAsync(vendorProfile);
            }

            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  EVENT TYPES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedEventTypeAsync()
        {
            if (!await context.EventTypes.AnyAsync())
            {
                var eventTypes = new List<EventType>
                {
                    new EventType { Id = Guid.NewGuid(), Name = "Wedding"    },
                    new EventType { Id = Guid.NewGuid(), Name = "Birthday"   },
                    new EventType { Id = Guid.NewGuid(), Name = "Graduation" }
                };

                await context.EventTypes.AddRangeAsync(eventTypes);
                await context.SaveChangesAsync();
            }
            else
            {
                var weeding = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Weeding");
                if (weeding != null) { weeding.Name = "Wedding"; await context.SaveChangesAsync(); }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ROLES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedRolesAsync()
        {
            string[] roles = { "Admin", "Vendor", "Customer" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<Guid>
                    {
                        Id             = Guid.NewGuid(),
                        Name           = role,
                        NormalizedName = role.ToUpper()
                    });
                }
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ADMIN USER
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedAdminUserAsync()
        {
            const string adminEmail = "admin@example.com";

            if (await userManager.FindByEmailAsync(adminEmail) != null)
                return;

            var newAdminUser = new ApplicationUser
            {
                Id                 = Guid.NewGuid(),
                FirstName          = "Mohamed",
                LastName           = "Tarek",
                UserName           = "admin",
                Email              = adminEmail,
                NormalizedEmail    = adminEmail.ToUpper(),
                NormalizedUserName = "ADMIN"
            };

            var result = await userManager.CreateAsync(newAdminUser, "Admin@123");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(newAdminUser, "Admin");
            else
                Console.WriteLine(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  VENDOR USER  (test / dummy)
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedVendorUserAsync()
        {
            const string vendorEmail = "vendor@example.com";

            if (await userManager.FindByEmailAsync(vendorEmail) != null)
                return;

            var vendorType = await context.VendorTypes.FirstOrDefaultAsync();
            if (vendorType == null) { Console.WriteLine("VendorType not found."); return; }

            var newVendorUser = new ApplicationUser
            {
                Id                 = Guid.NewGuid(),
                UserName           = "vendor",
                FirstName          = "Mohamed",
                LastName           = "Tarek",
                Email              = vendorEmail,
                NormalizedEmail    = vendorEmail.ToUpper(),
                NormalizedUserName = "VENDOR"
            };

            var result = await userManager.CreateAsync(newVendorUser, "Vendor@123");

            if (!result.Succeeded)
            {
                Console.WriteLine(string.Join(",", result.Errors.Select(e => e.Description)));
                return;
            }

            await userManager.AddToRoleAsync(newVendorUser, "Vendor");

            var vendorProfile = new Vendor
            {
                UserId          = newVendorUser.Id,
                BusinessName    = "Test Business",
                PortfolioLink   = "https://example.com",
                Description     = "Seeded Vendor",
                YearsInBusiness = 5,
                IsVerified      = true,
                VendorTypeId    = vendorType.Id,
                ProfilePicture  = "https://example.com/profile.jpg",
                Document        = "https://example.com/document.pdf",
                Address = new Address
                {
                    Street = "123 Test Street",
                    City   = "Cairo",
                    State  = "Cairo Governorate"
                },
                ServiceAreas = new List<ServiceArea>
                {
                    new ServiceArea { Id = Guid.NewGuid(), City = "Cairo", Region = "Nasr City",  Latitude = 30.0561m, Longitude = 31.3300m },
                    new ServiceArea { Id = Guid.NewGuid(), City = "Cairo", Region = "Maadi",      Latitude = 29.9602m, Longitude = 31.2569m },
                    new ServiceArea { Id = Guid.NewGuid(), City = "Giza",  Region = "Dokki",      Latitude = 30.0384m, Longitude = 31.2122m }
                }
            };

            await context.Vendors.AddAsync(vendorProfile);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CUSTOMER
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedCustomerAsync()
        {
            const string customerEmail = "customer@example.com";

            if (await userManager.FindByEmailAsync(customerEmail) != null)
                return;

            var newCustomerUser = new ApplicationUser
            {
                Id                 = Guid.NewGuid(),
                UserName           = "customer",
                FirstName          = "Mohamed",
                LastName           = "Tarek",
                Email              = customerEmail,
                NormalizedEmail    = customerEmail.ToUpper(),
                NormalizedUserName = "CUSTOMER",
                ReferralCode       = "REF12345"
            };

            var result = await userManager.CreateAsync(newCustomerUser, "Customer@123");

            if (result.Succeeded)
                await userManager.AddToRoleAsync(newCustomerUser, "Customer");
            else
                Console.WriteLine(string.Join(",", result.Errors.Select(e => e.Description)));
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SERVICE TYPES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedServiceTypesAsync()
        {
            if (await context.ServiceTypes.AnyAsync())
                return;

            var photographer = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Photographer");
            var caterer      = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Caterer");
            var decorator    = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Decorator");

            if (photographer == null || caterer == null || decorator == null)
            {
                Console.WriteLine("VendorTypes missing.");
                return;
            }

            var serviceTypes = new List<ServiceType>
            {
                new ServiceType { Id = Guid.NewGuid(), Name = "Photography", VendorTypeId = photographer.Id },
                new ServiceType { Id = Guid.NewGuid(), Name = "Catering",    VendorTypeId = caterer.Id      },
                new ServiceType { Id = Guid.NewGuid(), Name = "Decoration",  VendorTypeId = decorator.Id   }
            };

            await context.ServiceTypes.AddRangeAsync(serviceTypes);
            await context.SaveChangesAsync();
        }

        private async Task SeedServicesAsync()
        {
            if (await context.Services.AnyAsync())
                return;

            var vendors = await context.Vendors.ToListAsync();

            var photography = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Photography");
            var catering = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Catering");
            var decoration = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Decoration");

            var wedding = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Wedding");
            var birthday = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Birthday");
            var grad = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Graduation");

            if (photography == null || catering == null || decoration == null)
                return;

            var services = new List<Service>();

            var rand = new Random();

            // =========================
            // 1. PHOTOGRAPHY & MEDIA (30+)
            // =========================
            foreach (var v in vendors.Take(10))
            {
                services.AddRange(new[]
                {
            new Service {
                Id = Guid.NewGuid(),
                Name = "Wedding Cinematic Photography Package",
                Description = "Full-day cinematic wedding coverage with edited album",
                Price = rand.Next(4000, 12000),
                VendorId = v.UserId,
                ServiceTypeId = photography.Id,
                SetupDuration = 6,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Engagement Photoshoot",
                Description = "Outdoor engagement session with color grading",
                Price = rand.Next(1500, 5000),
                VendorId = v.UserId,
                ServiceTypeId = photography.Id,
                SetupDuration = 2,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Corporate Event Photography",
                Description = "Professional coverage for conferences and business events",
                Price = rand.Next(3000, 8000),
                VendorId = v.UserId,
                ServiceTypeId = photography.Id,
                SetupDuration = 5,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ wedding, grad }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Drone Aerial Coverage",
                Description = "4K drone footage for outdoor events",
                Price = rand.Next(2000, 6000),
                VendorId = v.UserId,
                ServiceTypeId = photography.Id,
                SetupDuration = 3,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ wedding, birthday, grad }
            }
        });
            }

            // =========================
            // 2. CATERING SERVICES (40+)
            // =========================
            foreach (var v in vendors.Take(15))
            {
                services.AddRange(new[]
                {
            new Service {
                Id = Guid.NewGuid(),
                Name = "Luxury Wedding Buffet",
                Description = "Full buffet catering for weddings (100–300 guests)",
                Price = rand.Next(15000, 60000),
                VendorId = v.UserId,
                ServiceTypeId = catering.Id,
                SetupDuration = 4,
                LeadTimeRequired = 3,
                EventTypes = new List<EventType>{ wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Corporate Lunch Buffet",
                Description = "Business catering with international menu",
                Price = rand.Next(5000, 20000),
                VendorId = v.UserId,
                ServiceTypeId = catering.Id,
                SetupDuration = 3,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ grad, wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Live Cooking Station",
                Description = "Interactive chef stations (pasta, grill, sushi)",
                Price = rand.Next(8000, 25000),
                VendorId = v.UserId,
                ServiceTypeId = catering.Id,
                SetupDuration = 5,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ wedding, birthday }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Dessert & Candy Bar Setup",
                Description = "Themed dessert table with premium sweets",
                Price = rand.Next(3000, 10000),
                VendorId = v.UserId,
                ServiceTypeId = catering.Id,
                SetupDuration = 2,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ wedding, birthday }
            }
        });
            }

            // =========================
            // 3. DECORATION (30+)
            // =========================
            foreach (var v in vendors.Take(12))
            {
                services.AddRange(new[]
                {
            new Service {
                Id = Guid.NewGuid(),
                Name = "Luxury Wedding Decoration Package",
                Description = "Full venue styling with floral + stage design",
                Price = rand.Next(10000, 50000),
                VendorId = v.UserId,
                ServiceTypeId = decoration.Id,
                SetupDuration = 8,
                LeadTimeRequired = 3,
                EventTypes = new List<EventType>{ wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Balloon Decoration Setup",
                Description = "Birthday themed balloon setups",
                Price = rand.Next(2000, 8000),
                VendorId = v.UserId,
                ServiceTypeId = decoration.Id,
                SetupDuration = 3,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ birthday }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Corporate Branding Backdrop",
                Description = "Branded stage & media wall design",
                Price = rand.Next(4000, 15000),
                VendorId = v.UserId,
                ServiceTypeId = decoration.Id,
                SetupDuration = 4,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ grad, wedding }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Floral Arrangement Package",
                Description = "Premium floral designs for halls & stages",
                Price = rand.Next(3000, 12000),
                VendorId = v.UserId,
                ServiceTypeId = decoration.Id,
                SetupDuration = 3,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ wedding }
            }
        });
            }

            // =========================
            // 4. EVENT PRODUCTION (NEW - 20+)
            // =========================
            foreach (var v in vendors.Take(10))
            {
                var productionType = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Photography"); // fallback reuse

                services.AddRange(new[]
                {
            new Service {
                Id = Guid.NewGuid(),
                Name = "Full Event Production Package",
                Description = "Complete sound, lighting, staging & coordination",
                Price = rand.Next(20000, 100000),
                VendorId = v.UserId,
                ServiceTypeId = productionType.Id,
                SetupDuration = 10,
                LeadTimeRequired = 4,
                EventTypes = new List<EventType>{ wedding, grad, birthday }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "LED Screen & Visual Setup",
                Description = "High resolution LED walls for events",
                Price = rand.Next(8000, 30000),
                VendorId = v.UserId,
                ServiceTypeId = productionType.Id,
                SetupDuration = 5,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ wedding, grad }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Professional Sound System",
                Description = "PA system for conferences and weddings",
                Price = rand.Next(5000, 20000),
                VendorId = v.UserId,
                ServiceTypeId = productionType.Id,
                SetupDuration = 3,
                LeadTimeRequired = 1,
                EventTypes = new List<EventType>{ wedding, birthday, grad }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Stage Lighting Package",
                Description = "Dynamic lighting design with DMX control",
                Price = rand.Next(6000, 25000),
                VendorId = v.UserId,
                ServiceTypeId = productionType.Id,
                SetupDuration = 4,
                LeadTimeRequired = 2,
                EventTypes = new List<EventType>{ wedding, grad }
            },

            new Service {
                Id = Guid.NewGuid(),
                Name = "Live Streaming Setup",
                Description = "Multi-camera live broadcast production",
                Price = rand.Next(7000, 30000),
                VendorId = v.UserId,
                ServiceTypeId = productionType.Id,
                SetupDuration = 6,
                LeadTimeRequired = 3,
                EventTypes = new List<EventType>{ wedding, grad }
            }
        });
            }

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  EVENT
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedEventAsync()
        {
            var user      = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");
            var eventType = context.EventTypes.FirstOrDefault(x => x.Name == "Wedding");
            var vendor    = context.ApplicationUsers.FirstOrDefault(v => v.Email == "vendor@example.com");

            if (user == null || eventType == null || vendor == null) return;

            var cateringService = context.Services.FirstOrDefault(s => s.Name == "Premium Catering");
            var photoService    = context.Services.FirstOrDefault(s => s.Name == "Wedding Photography Package");
            var decorService    = context.Services.FirstOrDefault(s => s.Name == "Wedding Decoration");

            if (cateringService == null || photoService == null || decorService == null) return;

            if (context.Events.Any(e => e.Title == "Luxury Wedding Cairo 2026")) return;

            var newEvent = new Event
            {
                Id          = Guid.NewGuid(),
                UserId      = user.Id,
                EventTypeId = eventType.Id,
                Title       = "Luxury Wedding Cairo 2026",
                EventDate   = DateTime.UtcNow.AddMonths(3),
                Location    = new Address { City = "Cairo", State = "Giza", Street = "Pyramids Road" },
                TotalBudget = 75000m,
                GuestCount  = 250,
                Notes       = "Premium wedding with full vendor coordination",
                EventStatus = "Planned",
                EventItems  = new List<EventItem>
                {
                    new EventItem { Id = Guid.NewGuid(), ServiceId = cateringService.Id, Service = cateringService, Price = cateringService.Price, Quantity = 1, ItemStatus = "Approved", RejectionReason = null },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,    Service = photoService,    Price = photoService.Price,    Quantity = 1, ItemStatus = "Pending"  },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = decorService.Id,    Service = decorService,    Price = decorService.Price,    Quantity = 1, ItemStatus = "Pending"  }
                }
            };

            await context.Events.AddAsync(newEvent);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PACKAGES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedPackagesAsync()
        {
            if (await context.Packages.AnyAsync())
                return;

            var vendor = await context.Vendors.FirstOrDefaultAsync();
            if (vendor == null) { Console.WriteLine("Vendor missing for package seeding."); return; }

            var packages = new List<Package>
            {
                new Package { Id = Guid.NewGuid(), Name = "Basic Wedding Package",   Description = "Basic wedding package",   Price = 10000, Discount = 10, VendorId = vendor.UserId, Items = new List<string> { "Photography", "Decoration", "Catering" } },
                new Package { Id = Guid.NewGuid(), Name = "Premium Wedding Package", Description = "Premium wedding package", Price = 25000, Discount = 15, VendorId = vendor.UserId, Items = new List<string> { "Luxury Photography", "Luxury Decoration", "Luxury Catering" } }
            };

            await context.Packages.AddRangeAsync(packages);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  NOTIFICATIONS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedNotificationAsnyc()
        {
            if (await context.Notifications.AnyAsync())
                return;

            var vendor   = context.ApplicationUsers.FirstOrDefault(u => u.Email == "vendor@example.com");
            var customer = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");

            var notifications = new List<Notification>
            {
                new Notification { Id = Guid.NewGuid(), UserId = vendor.Id,   Message = "Welcome to our event management platform!", Title = "Test",             Type = NotificationType.ACCOUNT_ACCEPTED, IsRead = false, CreatedAt = DateTime.UtcNow },
                new Notification { Id = Guid.NewGuid(), UserId = vendor.Id,   Message = "Your vendor account has been approved.",     Title = "Account Approved", Type = NotificationType.ACCOUNT_ACCEPTED, IsRead = false, CreatedAt = DateTime.UtcNow },
                new Notification { Id = Guid.NewGuid(), UserId = customer.Id, Message = "You received a new booking request.",        Title = "New Booking",      Type = NotificationType.ORDER_PLACED,     IsRead = false, CreatedAt = DateTime.UtcNow }
            };

            await context.Notifications.AddRangeAsync(notifications);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ORDER
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedOrderAsync()
        {
            var user          = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");
            var existingEvent = context.Events.FirstOrDefault(e => e.Title == "Luxury Wedding Cairo 2026");

            if (user == null || existingEvent == null) return;
            if (context.Orders.Any(o => o.EventId == existingEvent.Id)) return;

            var order = new Order
            {
                Id              = Guid.NewGuid(),
                UserId          = user.Id,
                EventId         = existingEvent.Id,
                Amount          = 47000m,
                Currency        = "EGP",
                PaymentIntentId = $"PAYMOB_{Guid.NewGuid():N}",
                PaymentStatus   = "Pending",
                Appointment     = DateTime.UtcNow.AddDays(10),
                ShippingAddress = new Address { City = "Cairo", State = "Giza", Street = "Pyramids Road" },
                CreatedAt       = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }
    }
}