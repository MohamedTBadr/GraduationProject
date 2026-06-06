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
            await context.Messages.ExecuteDeleteAsync();
            await context.Conversations.ExecuteDeleteAsync();
            await context.Database.MigrateAsync();

            var strategy = context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    Console.WriteLine("Database seeding started...");
                    await SeedVendorTypeAsync();
                    await SeedRolesAsync();
                    await SeedAdminUserAsync();
                    await SeedVendorUserAsync();
                    await SeedVenueVendorsAsync();
                    await SeedCoworkingSpaceVendorsAsync();
                    await SeedProductionVendorsAsync();
                    await SeedCateringVendorsAsync();
                    await SeedDecorationVendorsAsync();
                    await SeedMediaVendorsAsync();
                    await SeedFurnitureVendorsAsync();
                    await SeedTransportationVendorsAsync();
                    await SeedPrintingVendorsAsync();
                    await SeedEntertainmentVendorsAsync();
                    await SeedSecurityVendorsAsync();
                    await SeedMakeupVendorsAsync();
                    await SeedKidsVendorsAsync();
                    await SeedCorporateVendorsAsync();
                    await SeedCustomerAsync();
                    await SeedServiceTypesAsync();
                    await SeedEventTypeAsync();
                    await SeedServicesAsync();
                    await SeedPackagesAsync();
                    await SeedNotificationAsnyc();
                    await SeedEventAsync();
                    await SeedOrderAsync();
                    await SeedCilantroDataAsync();

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
        new VendorType { Name = "Furniture & Setup" },
        new VendorType { Name = "Media" },
        new VendorType { Name = "Catering" },
        new VendorType { Name = "Venue" },
        new VendorType { Name = "Entertainment" },
        new VendorType { Name = "Printing" },
        new VendorType { Name = "Transportation" },
        new VendorType { Name = "Coworking Space" },
        new VendorType { Name = "Production" },
        // ── new ──
        new VendorType { Name = "Decoration & Floral" },
        new VendorType { Name = "Security & Staffing" },
        new VendorType { Name = "Makeup & Bridal" },

        new VendorType { Name = "Kids Activities" },
        new VendorType { Name = "Corporate Services" },
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
        "catering.willyskitchen@placeholder.com",

        // Cake Designers
        "happiness@ninosbakeryeg.com",
        "cake.cairocakes@placeholder.com",
        "cake.pharaonic@placeholder.com",
        "cake.sweetgarden@placeholder.com",
        "cake.royal@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => cateringEmails.Contains(u.Email)))
                return;

            var catererType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Catering");

            if (catererType == null)
            {
                Console.WriteLine("[CateringSeeding] VendorType 'Catering' not found.");
                return;
            }

            var cateringData = new[]
            {
        // ─────────────────────────────
        // Catering Vendors
        // ─────────────────────────────

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
        },

        // ─────────────────────────────
        // Cake Designers
        // ─────────────────────────────

        new
        {
            BusinessName = "Nino's Bakery",
            Phone = "+201023147888",
            Email = "happiness@ninosbakeryeg.com",
            Street = "11 El Sheikh El Ni'ma St., Nasr City",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "Nasr City", 30.0490m, 31.3303m)
            }
        },

        new
        {
            BusinessName = "Cairo Cakes Co",
            Phone = "",
            Email = "cake.cairocakes@placeholder.com",
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
            BusinessName = "Pharaonic Pastries",
            Phone = "01000000110",
            Email = "cake.pharaonic@placeholder.com",
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
            BusinessName = "Sweet Garden Cairo",
            Phone = "",
            Email = "cake.sweetgarden@placeholder.com",
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
            BusinessName = "Royal Bakeries Cairo",
            Phone = "01000000111",
            Email = "cake.royal@placeholder.com",
            Street = "New Cairo",
            City = "Cairo",
            State = "Cairo Governorate",
            Regions = new[]
            {
                ("Cairo", "New Cairo", 30.0120m, 31.4354m)
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
        //  DECORATION & FLORAL VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedDecorationVendorsAsync()
        {
            var decorEmails = new[]
            {
        "info@flowerpowerdesign.com",
        "info@tailordevents.com",
        "events@daliaelhaggar.com",
        "decor.dreamy@placeholder.com",
        "decor.gardenia@placeholder.com",
        "decor.elegant@placeholder.com",
        "decor.blossom@placeholder.com",
        "decor.lux@placeholder.com",
        "decor.elite@placeholder.com",
        "decor.magenta@placeholder.com",
        "decor.gold@placeholder.com",
        "decor.vintage@placeholder.com",
        "decor.cairocreative@placeholder.com",
        "decor.nileoasis@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => decorEmails.Contains(u.Email)))
                return;

            var decorType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Decoration & Floral");

            if (decorType == null)
            {
                Console.WriteLine("[DecorationSeeding] VendorType 'Decoration & Floral' not found.");
                return;
            }

            var decorationData = new[]
            {
        new {
            BusinessName = "Flower Power Design",
            Phone        = "01223904907",
            Email        = "info@flowerpowerdesign.com",
            Street       = "14 Wadi El Nile St.",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Tailor'd Events LLC",
            Phone        = "+20 1272377238",
            Email        = "info@tailordevents.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Dalia El Haggar Florist",
            Phone        = "01222167048",
            Email        = "events@daliaelhaggar.com",
            Street       = "Unknown Street",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Dreamy Decor Cairo",
            Phone        = "",
            Email        = "decor.dreamy@placeholder.com",
            Street       = "Al Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Gardenia Floral Design",
            Phone        = "01000000001",
            Email        = "decor.gardenia@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Elegant Events Decor",
            Phone        = "",
            Email        = "decor.elegant@placeholder.com",
            Street       = "Sheikh Zayed",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m) }
        },
        new {
            BusinessName = "Blossom Creations",
            Phone        = "01000000002",
            Email        = "decor.blossom@placeholder.com",
            Street       = "New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        },
        new {
            BusinessName = "Lux Floral Egypt",
            Phone        = "",
            Email        = "decor.lux@placeholder.com",
            Street       = "Dokki",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Dokki", 30.0419m, 31.2047m) }
        },
        new {
            BusinessName = "Elite Event Stylists",
            Phone        = "01000000003",
            Email        = "decor.elite@placeholder.com",
            Street       = "Downtown Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Magenta Floral",
            Phone        = "",
            Email        = "decor.magenta@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0561m, 31.3300m) }
        },
        new {
            BusinessName = "Golden Petals",
            Phone        = "01000000004",
            Email        = "decor.gold@placeholder.com",
            Street       = "Heliopolis",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Heliopolis", 30.0868m, 31.3235m) }
        },
        new {
            BusinessName = "Vintage Blooms",
            Phone        = "",
            Email        = "decor.vintage@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Cairo Creative Decor",
            Phone        = "01000000005",
            Email        = "decor.cairocreative@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "Nile Oasis Events",
            Phone        = "",
            Email        = "decor.nileoasis@placeholder.com",
            Street       = "Imbaba",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Imbaba", 30.0691m, 31.2140m) }
        }
    };

            await SeedVendorListAsync(
                decorationData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                decorType,
                "DecorationSeeding");

            Console.WriteLine("[DecorationSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  MEDIA / PHOTOGRAPHY VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedMediaVendorsAsync()
        {
            var mediaEmails = new[]
            {
        "info@splashwedding.com",
        "media.fairytale@placeholder.com",
        "media.nation@placeholder.com",
        "media.goldenframe@placeholder.com",
        "media.artistic@placeholder.com",
        "media.magic@placeholder.com",
        "media.sunrise@placeholder.com",
        "media.visionary@placeholder.com",
        "media.capture@placeholder.com",
        "media.focus@placeholder.com",
        "media.cinema@placeholder.com",
        "media.flash@placeholder.com",
        "media.moments@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => mediaEmails.Contains(u.Email)))
                return;

            var mediaType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Media");

            if (mediaType == null)
            {
                Console.WriteLine("[MediaSeeding] VendorType 'Media' not found.");
                return;
            }

            var mediaData = new[]
            {
        new {
            BusinessName = "Splash Wedding Studios",
            Phone        = "+201111091999",
            Email        = "info@splashwedding.com",
            Street       = "New Cairo Housing",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        },
        new {
            BusinessName = "Fairytale Photography Egypt",
            Phone        = "01000000010",
            Email        = "media.fairytale@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "MediaNation Cairo",
            Phone        = "",
            Email        = "media.nation@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Golden Frame Photographers",
            Phone        = "01000000011",
            Email        = "media.goldenframe@placeholder.com",
            Street       = "Heliopolis",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Heliopolis", 30.0876m, 31.3220m) }
        },
        new {
            BusinessName = "Artistic Lens Cairo",
            Phone        = "",
            Email        = "media.artistic@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "Magic Moments Studio",
            Phone        = "01000000012",
            Email        = "media.magic@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0561m, 31.3300m) }
        },
        new {
            BusinessName = "Sunrise Media",
            Phone        = "",
            Email        = "media.sunrise@placeholder.com",
            Street       = "New Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9598m, 31.2556m) }
        },
        new {
            BusinessName = "Visionary Studios",
            Phone        = "01000000013",
            Email        = "media.visionary@placeholder.com",
            Street       = "Zahraa Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Capture Cairo",
            Phone        = "",
            Email        = "media.capture@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Focus Event Photography",
            Phone        = "01000000014",
            Email        = "media.focus@placeholder.com",
            Street       = "Sheikh Zayed",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m) }
        },
        new {
            BusinessName = "Cinema Frames",
            Phone        = "",
            Email        = "media.cinema@placeholder.com",
            Street       = "Heliopolis",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Heliopolis", 30.0876m, 31.3220m) }
        },
        new {
            BusinessName = "Flash Photography",
            Phone        = "01000000015",
            Email        = "media.flash@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0561m, 31.3300m) }
        },
        new {
            BusinessName = "Moments Photography",
            Phone        = "",
            Email        = "media.moments@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        }
    };

            await SeedVendorListAsync(
                mediaData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                mediaType,
                "MediaSeeding");

            Console.WriteLine("[MediaSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  FURNITURE & RENTALS VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedFurnitureVendorsAsync()
        {
            var furnitureEmails = new[]
            {
        "info@vendegypt.com",
        "rental.eventessentials@placeholder.com",
        "rental.tablechair@placeholder.com",
        "rental.tentscity@placeholder.com",
        "rental.cairoparty@placeholder.com",
        "rental.luxtent@placeholder.com",
        "rental.basmenatents@placeholder.com",
        "rental.bluelile@placeholder.com",
        "rental.nile@placeholder.com",
        "rental.eventdecor@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => furnitureEmails.Contains(u.Email)))
                return;

            var furnitureType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Furniture & Setup");

            if (furnitureType == null)
            {
                Console.WriteLine("[FurnitureSeeding] VendorType 'Furniture & Setup' not found.");
                return;
            }

            var furnitureData = new[]
            {
        new {
            BusinessName = "Vend Egypt Rentals",
            Phone        = "+201124334178",
            Email        = "info@vendegypt.com",
            Street       = "Place Tower, New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0230m, 31.5006m) }
        },
        new {
            BusinessName = "Event Essentials Rental",
            Phone        = "",
            Email        = "rental.eventessentials@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Table & Chair Egypt",
            Phone        = "01000000050",
            Email        = "rental.tablechair@placeholder.com",
            Street       = "6th October",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "6th of October", 29.9285m, 30.9188m) }
        },
        new {
            BusinessName = "Tent City Cairo",
            Phone        = "",
            Email        = "rental.tentscity@placeholder.com",
            Street       = "New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        },
        new {
            BusinessName = "Cairo Party Rentals",
            Phone        = "01000000051",
            Email        = "rental.cairoparty@placeholder.com",
            Street       = "Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Lux Tent & Chair",
            Phone        = "",
            Email        = "rental.luxtent@placeholder.com",
            Street       = "Dokki",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Dokki", 30.0419m, 31.2047m) }
        },
        new {
            BusinessName = "Basmena Tents",
            Phone        = "01000000052",
            Email        = "rental.basmenatents@placeholder.com",
            Street       = "Zahraa Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Blue Nile Events Rentals",
            Phone        = "",
            Email        = "rental.bluelile@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Nile Rental Co.",
            Phone        = "01000000053",
            Email        = "rental.nile@placeholder.com",
            Street       = "Sheikh Zayed",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m) }
        },
        new {
            BusinessName = "Event Decor Rentals",
            Phone        = "",
            Email        = "rental.eventdecor@placeholder.com",
            Street       = "Tahrir Square",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        }
    };

            await SeedVendorListAsync(
                furnitureData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                furnitureType,
                "FurnitureSeeding");

            Console.WriteLine("[FurnitureSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  TRANSPORTATION VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedTransportationVendorsAsync()
        {
            var transportEmails = new[]
            {
        "info@premiumlimousine-eg.com",
        "transport.lux@placeholder.com",
        "transport.valley@placeholder.com",
        "transport.sphinxshuttle@placeholder.com",
        "transport.cairoexecutive@placeholder.com",
        "transport.goldentulip@placeholder.com",
        "transport.aircairo@placeholder.com",
        "transport.deluxebus@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => transportEmails.Contains(u.Email)))
                return;

            var transportationType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Transportation");

            if (transportationType == null)
            {
                Console.WriteLine("[TransportationSeeding] VendorType 'Transportation' not found.");
                return;
            }

            var transportData = new[]
            {
        new {
            BusinessName = "Egypt Premium Limousine",
            Phone        = "+201129119919",
            Email        = "info@premiumlimousine-eg.com",
            Street       = "350 Gardinia City, Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Lux Transport Cairo",
            Phone        = "01000000060",
            Email        = "transport.lux@placeholder.com",
            Street       = "Dokki",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Dokki", 30.0419m, 31.2047m) }
        },
        new {
            BusinessName = "Valley Nile Shuttle",
            Phone        = "",
            Email        = "transport.valley@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Sphinx Shuttle Services",
            Phone        = "01000000061",
            Email        = "transport.sphinxshuttle@placeholder.com",
            Street       = "Sheikh Zayed",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m) }
        },
        new {
            BusinessName = "Cairo Executive Chauffeurs",
            Phone        = "",
            Email        = "transport.cairoexecutive@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Golden Tulip Transfers",
            Phone        = "01000000062",
            Email        = "transport.goldentulip@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "Air Cairo Chauffeurs",
            Phone        = "",
            Email        = "transport.aircairo@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Deluxe Bus Lines",
            Phone        = "01000000063",
            Email        = "transport.deluxebus@placeholder.com",
            Street       = "New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        }
    };

            await SeedVendorListAsync(
                transportData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                transportationType,
                "TransportationSeeding");

            Console.WriteLine("[TransportationSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  PRINTING & INVITATIONS VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedPrintingVendorsAsync()
        {
            var printingEmails = new[]
            {
        "print.goldenpress@placeholder.com",
        "print.eliteinvites@placeholder.com",
        "print.cairoprint@placeholder.com",
        "print.royalsol@placeholder.com",
        "print.lotus@placeholder.com",
        "print.bluenile@placeholder.com",
        "print.invitedesign@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => printingEmails.Contains(u.Email)))
                return;

            var printingType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Printing");

            if (printingType == null)
            {
                Console.WriteLine("[PrintingSeeding] VendorType 'Printing' not found.");
                return;
            }

            var printingData = new[]
            {
        new {
            BusinessName = "Golden Press Cairo",
            Phone        = "",
            Email        = "print.goldenpress@placeholder.com",
            Street       = "10 Ramses St.",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Elite Invitations",
            Phone        = "01000000070",
            Email        = "print.eliteinvites@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "Cairo Print Design",
            Phone        = "",
            Email        = "print.cairoprint@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Royal Print Solutions",
            Phone        = "01000000071",
            Email        = "print.royalsol@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Lotus Printing House",
            Phone        = "",
            Email        = "print.lotus@placeholder.com",
            Street       = "Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Blue Nile Press",
            Phone        = "01000000072",
            Email        = "print.bluenile@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Egypt Invitation Design",
            Phone        = "",
            Email        = "print.invitedesign@placeholder.com",
            Street       = "Heliopolis",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Heliopolis", 30.0876m, 31.3220m) }
        }
    };

            await SeedVendorListAsync(
                printingData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                printingType,
                "PrintingSeeding");

            Console.WriteLine("[PrintingSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  ENTERTAINMENT VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedEntertainmentVendorsAsync()
        {
            var entertainmentEmails = new[]
            {
        "ent.nilebeats@placeholder.com",
        "ent.pyramid@placeholder.com",
        "ent.orientalbeats@placeholder.com",
        "ent.desertrose@placeholder.com",
        "ent.urbangrooves@placeholder.com",
        "ent.midnight@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => entertainmentEmails.Contains(u.Email)))
                return;

            var entertainmentType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Entertainment");

            if (entertainmentType == null)
            {
                Console.WriteLine("[EntertainmentSeeding] VendorType 'Entertainment' not found.");
                return;
            }

            var entertainmentData = new[]
            {
        new {
            BusinessName = "Nile Beats Live Band",
            Phone        = "01000000080",
            Email        = "ent.nilebeats@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Pyramid DJ Services",
            Phone        = "",
            Email        = "ent.pyramid@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "Oriental Beats Entertainment",
            Phone        = "01000000081",
            Email        = "ent.orientalbeats@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Desert Rose Band",
            Phone        = "",
            Email        = "ent.desertrose@placeholder.com",
            Street       = "New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        },
        new {
            BusinessName = "Urban Grooves Entertainment",
            Phone        = "01000000082",
            Email        = "ent.urbangrooves@placeholder.com",
            Street       = "Sheikh Zayed",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Sheikh Zayed", 30.0131m, 30.9744m) }
        },
        new {
            BusinessName = "Midnight DJs Cairo",
            Phone        = "",
            Email        = "ent.midnight@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        }
    };

            await SeedVendorListAsync(
                entertainmentData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                entertainmentType,
                "EntertainmentSeeding");

            Console.WriteLine("[EntertainmentSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SECURITY & STAFFING VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedSecurityVendorsAsync()
        {
            var securityEmails = new[]
            {
        "security.nileevent@placeholder.com",
        "security.safeguard@placeholder.com",
        "security.egyptguards@placeholder.com",
        "security.pharaoh@placeholder.com",
        "security.vipse@placeholder.com",
        "security.shadow@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => securityEmails.Contains(u.Email)))
                return;

            var securityType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Security & Staffing");

            if (securityType == null)
            {
                Console.WriteLine("[SecuritySeeding] VendorType 'Security & Staffing' not found.");
                return;
            }

            var securityData = new[]
            {
        new {
            BusinessName = "Nile Event Security",
            Phone        = "01000000090",
            Email        = "security.nileevent@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Safeguard Egypt",
            Phone        = "",
            Email        = "security.safeguard@placeholder.com",
            Street       = "Downtown Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        },
        new {
            BusinessName = "Egyptian Guards Co",
            Phone        = "01000000091",
            Email        = "security.egyptguards@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Pharaoh Security",
            Phone        = "",
            Email        = "security.pharaoh@placeholder.com",
            Street       = "Giza",
            City         = "Giza",
            State        = "Giza Governorate",
            Regions      = new[] { ("Giza", "Dokki", 30.0419m, 31.2047m) }
        },
        new {
            BusinessName = "VIP Secure Egypt",
            Phone        = "01000000092",
            Email        = "security.vipse@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Shadow Security",
            Phone        = "",
            Email        = "security.shadow@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        }
    };

            await SeedVendorListAsync(
                securityData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                securityType,
                "SecuritySeeding");

            Console.WriteLine("[SecuritySeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  MAKEUP & BRIDAL VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedMakeupVendorsAsync()
        {
            var makeupEmails = new[]
            {
        "beauty.golden@placeholder.com",
        "beauty.bridal@placeholder.com",
        "beauty.divine@placeholder.com",
        "beauty.nilewedding@placeholder.com",
        "beauty.oriental@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => makeupEmails.Contains(u.Email)))
                return;

            var makeupType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Makeup & Bridal");

            if (makeupType == null)
            {
                Console.WriteLine("[MakeupSeeding] VendorType 'Makeup & Bridal' not found.");
                return;
            }

            var makeupData = new[]
            {
        new {
            BusinessName = "Golden Glamour Makeup",
            Phone        = "01000000100",
            Email        = "beauty.golden@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Cairo Bridal Beauty",
            Phone        = "",
            Email        = "beauty.bridal@placeholder.com",
            Street       = "Heliopolis",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Heliopolis", 30.0876m, 31.3220m) }
        },
        new {
            BusinessName = "Divine Makeup Artistry",
            Phone        = "01000000101",
            Email        = "beauty.divine@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Nile Wedding Hair & Makeup",
            Phone        = "",
            Email        = "beauty.nilewedding@placeholder.com",
            Street       = "Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Oriental Makeup Studio",
            Phone        = "01000000102",
            Email        = "beauty.oriental@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        }
    };

            await SeedVendorListAsync(
                makeupData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                makeupType,
                "MakeupSeeding");

            Console.WriteLine("[MakeupSeeding] Completed.");
        }



        // ─────────────────────────────────────────────────────────────────────
        //  KIDS ACTIVITIES VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedKidsVendorsAsync()
        {
            var kidsEmails = new[]
            {
        "kids.kingdom@placeholder.com",
        "kids.happycastle@placeholder.com",
        "kids.partyjungle@placeholder.com",
        "kids.funtime@placeholder.com",
        "kids.littlerascals@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => kidsEmails.Contains(u.Email)))
                return;

            var kidsType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Kids Activities");

            if (kidsType == null)
            {
                Console.WriteLine("[KidsSeeding] VendorType 'Kids Activities' not found.");
                return;
            }

            var kidsData = new[]
            {
        new {
            BusinessName = "Kids Kingdom Cairo",
            Phone        = "01000000120",
            Email        = "kids.kingdom@placeholder.com",
            Street       = "Maadi",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Maadi", 29.9602m, 31.2569m) }
        },
        new {
            BusinessName = "Happy Castle Play",
            Phone        = "",
            Email        = "kids.happycastle@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Party Jungle Egypt",
            Phone        = "01000000121",
            Email        = "kids.partyjungle@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Fun Time Zone",
            Phone        = "",
            Email        = "kids.funtime@placeholder.com",
            Street       = "Nasr City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Nasr City", 30.0490m, 31.3303m) }
        },
        new {
            BusinessName = "Little Rascals Party",
            Phone        = "01000000122",
            Email        = "kids.littlerascals@placeholder.com",
            Street       = "Downtown Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Downtown", 30.0444m, 31.2357m) }
        }
    };

            await SeedVendorListAsync(
                kidsData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                kidsType,
                "KidsSeeding");

            Console.WriteLine("[KidsSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CORPORATE SERVICES VENDORS
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedCorporateVendorsAsync()
        {
            var corporateEmails = new[]
            {
        "corp.nilespeakers@placeholder.com",
        "corp.summit@placeholder.com",
        "corp.emcpros@placeholder.com",
        "corp.interwork@placeholder.com"
    };

            if (await context.ApplicationUsers.AnyAsync(u => corporateEmails.Contains(u.Email)))
                return;

            var corporateType = await context.VendorTypes
                .FirstOrDefaultAsync(v => v.Name == "Corporate Services");

            if (corporateType == null)
            {
                Console.WriteLine("[CorporateSeeding] VendorType 'Corporate Services' not found.");
                return;
            }

            var corporateData = new[]
            {
        new {
            BusinessName = "Nile Speakers Bureau",
            Phone        = "01000000130",
            Email        = "corp.nilespeakers@placeholder.com",
            Street       = "Zamalek",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Zamalek", 30.0626m, 31.2197m) }
        },
        new {
            BusinessName = "Summit Translators Egypt",
            Phone        = "",
            Email        = "corp.summit@placeholder.com",
            Street       = "Garden City",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Garden City", 30.0393m, 31.2280m) }
        },
        new {
            BusinessName = "Event MC Pros",
            Phone        = "01000000131",
            Email        = "corp.emcpros@placeholder.com",
            Street       = "Mohandessin",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "Mohandessin", 30.0466m, 31.1956m) }
        },
        new {
            BusinessName = "InterWork Translating",
            Phone        = "",
            Email        = "corp.interwork@placeholder.com",
            Street       = "New Cairo",
            City         = "Cairo",
            State        = "Cairo Governorate",
            Regions      = new[] { ("Cairo", "New Cairo", 30.0120m, 31.4354m) }
        }
    };

            await SeedVendorListAsync(
                corporateData.Select(v => (v.BusinessName, v.Phone, v.Email, v.Street, v.City, v.State, v.Regions)),
                corporateType,
                "CorporateSeeding");

            Console.WriteLine("[CorporateSeeding] Completed.");
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
                    Id = Guid.NewGuid(),
                    UserName = v.Email,
                    FirstName = v.BusinessName,
                    LastName = vendorType.Name,
                    Email = v.Email,
                    NormalizedEmail = v.Email.ToUpper(),
                    NormalizedUserName = v.Email.ToUpper(),
                    PhoneNumber = v.Phone,

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
                        Id = Guid.NewGuid(),
                        City = r.Item1,
                        Region = r.Item2,
                        Latitude = r.Item3,
                        Longitude = r.Item4
                    })
                    .ToList();

                var vendorProfile = new Vendor
                {
                    UserId = appUser.Id,
                    BusinessName = v.BusinessName,
                    PortfolioLink = string.Empty,
                    Description = $"{v.BusinessName} – {vendorType.Name} provider.",
                    YearsInBusiness = 0,
                    IsVerified = true,
                    VendorTypeId = vendorType.Id,
                    ProfilePicture = string.Empty,
                    Document = string.Empty,
                    Address = new Address
                    {
                        Street = v.Street,
                        City = v.City,
                        State = v.State
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
                    new EventType { Id = Guid.NewGuid(), Name = "Wedding"          },
                    new EventType { Id = Guid.NewGuid(), Name = "Birthday"         },
                    new EventType { Id = Guid.NewGuid(), Name = "Graduation"       },
                    new EventType { Id = Guid.NewGuid(), Name = "Corporate Event"  },
                    new EventType { Id = Guid.NewGuid(), Name = "Conference"       },
                    new EventType { Id = Guid.NewGuid(), Name = "Kids Party"       },
                    new EventType { Id = Guid.NewGuid(), Name = "Exhibition"       },
                    new EventType { Id = Guid.NewGuid(), Name = "Concert"          },
                };

                await context.EventTypes.AddRangeAsync(eventTypes);
                await context.SaveChangesAsync();
            }
            else
            {
                var weeding = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Weeding");
                if (weeding != null) { weeding.Name = "Wedding"; await context.SaveChangesAsync(); }

                // Add missing event types if they don't exist yet
                var missingNames = new[] { "Corporate Event", "Conference", "Kids Party", "Exhibition", "Concert" };
                foreach (var name in missingNames)
                {
                    if (!await context.EventTypes.AnyAsync(e => e.Name == name))
                    {
                        await context.EventTypes.AddAsync(new EventType { Id = Guid.NewGuid(), Name = name });
                    }
                }
                await context.SaveChangesAsync();
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
                        Id = Guid.NewGuid(),
                        Name = role,
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
                Id = Guid.NewGuid(),
                UserName = "vendor",
                FirstName = "Mohamed",
                LastName = "Tarek",
                Email = vendorEmail,
                NormalizedEmail = vendorEmail.ToUpper(),
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
                    State = "Cairo Governorate"
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
                Id = Guid.NewGuid(),
                UserName = "customer",
                FirstName = "Mohamed",
                LastName = "Tarek",
                Email = customerEmail,
                NormalizedEmail = customerEmail.ToUpper(),
                NormalizedUserName = "CUSTOMER",
                ReferralCode = "REF12345"
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

            var furniture = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Furniture & Setup");
            var media = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Media");
            var catering = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Catering");
            var entertainment = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Entertainment");
            var printing = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Printing");
            var transportation = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Transportation");
            var venue = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Venue");
            var coworking = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Coworking Space");
            var production = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Production");
            var decoration = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Decoration & Floral");
            var security = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Security & Staffing");
            var makeup = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Makeup & Bridal");
            var kids = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Kids Activities");
            var corporate = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Corporate Services");

            if (furniture == null || media == null || catering == null ||
                entertainment == null || printing == null || transportation == null || venue == null)
            {
                Console.WriteLine("[ServiceTypeSeeding] One or more core VendorTypes missing.");
                return;
            }

            var serviceTypes = new List<ServiceType>
            {
                // ── Furniture & Setup (6) ──
                new ServiceType { Name = "Chairs Rental",         VendorTypeId = furniture.Id },
                new ServiceType { Name = "Tables Rental",         VendorTypeId = furniture.Id },
                new ServiceType { Name = "Stage Setup",           VendorTypeId = furniture.Id },
                new ServiceType { Name = "Lighting System Setup", VendorTypeId = furniture.Id },
                new ServiceType { Name = "Sound System Setup",    VendorTypeId = furniture.Id },
                new ServiceType { Name = "LED Screens",           VendorTypeId = furniture.Id },

                // ── Media (4) ──
                new ServiceType { Name = "Photographer",          VendorTypeId = media.Id },
                new ServiceType { Name = "Videographer",          VendorTypeId = media.Id },
                new ServiceType { Name = "Drone Videography",     VendorTypeId = media.Id },
                new ServiceType { Name = "Photobooth",            VendorTypeId = media.Id },

                // ── Catering (6) ──
                new ServiceType { Name = "Open Buffet",           VendorTypeId = catering.Id },
                new ServiceType { Name = "Set Menu",              VendorTypeId = catering.Id },
                new ServiceType { Name = "Live Cooking",          VendorTypeId = catering.Id },
                new ServiceType { Name = "Drinks Corner",         VendorTypeId = catering.Id },
                new ServiceType { Name = "Dessert & Candy Bar",   VendorTypeId = catering.Id },
                new ServiceType { Name = "Cake Design",           VendorTypeId = catering.Id },

                // ── Venue (4) ──
                new ServiceType { Name = "Indoor Venue",          VendorTypeId = venue.Id },
                new ServiceType { Name = "Outdoor Venue",         VendorTypeId = venue.Id },
                new ServiceType { Name = "Rooftop Venue",         VendorTypeId = venue.Id },
                new ServiceType { Name = "Conference Hall",       VendorTypeId = venue.Id },

                // ── Entertainment (5) ──
                new ServiceType { Name = "DJ",                    VendorTypeId = entertainment.Id },
                new ServiceType { Name = "Live Band",             VendorTypeId = entertainment.Id },
                new ServiceType { Name = "Magic Show",            VendorTypeId = entertainment.Id },
                new ServiceType { Name = "Comedian / MC",         VendorTypeId = entertainment.Id },
                new ServiceType { Name = "Dance Performance",     VendorTypeId = entertainment.Id },

                // ── Printing (4) ──
                new ServiceType { Name = "Printed Invitations",   VendorTypeId = printing.Id },
                new ServiceType { Name = "Digital Invitations",   VendorTypeId = printing.Id },
                new ServiceType { Name = "Banners",               VendorTypeId = printing.Id },
                new ServiceType { Name = "Customized Cards",      VendorTypeId = printing.Id },

                // ── Transportation (4) ──
                new ServiceType { Name = "Luxury Transport",      VendorTypeId = transportation.Id },
                new ServiceType { Name = "Limousine",             VendorTypeId = transportation.Id },
                new ServiceType { Name = "Shuttle Transport",     VendorTypeId = transportation.Id },
                new ServiceType { Name = "VIP Motorcade",         VendorTypeId = transportation.Id },
            };

            // ── Coworking Space (4) ──
            if (coworking != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Private Office",        VendorTypeId = coworking.Id },
                    new ServiceType { Name = "Meeting Room",          VendorTypeId = coworking.Id },
                    new ServiceType { Name = "Hot Desk",              VendorTypeId = coworking.Id },
                    new ServiceType { Name = "Event Space Rental",    VendorTypeId = coworking.Id },
                });

            // ── Production (4) ──
            if (production != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Event Management",      VendorTypeId = production.Id },
                    new ServiceType { Name = "Stage Production",      VendorTypeId = production.Id },
                    new ServiceType { Name = "Exhibition Setup",      VendorTypeId = production.Id },
                    new ServiceType { Name = "Conference Production",  VendorTypeId = production.Id },
                });

            // ── Decoration & Floral (5) ──
            if (decoration != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Floral Arrangements",   VendorTypeId = decoration.Id },
                    new ServiceType { Name = "Balloon Decoration",    VendorTypeId = decoration.Id },
                    new ServiceType { Name = "Table Centerpieces",    VendorTypeId = decoration.Id },
                    new ServiceType { Name = "Wedding Arch & Backdrop", VendorTypeId = decoration.Id },
                    new ServiceType { Name = "Full Venue Decoration",  VendorTypeId = decoration.Id },
                });

            // ── Security & Staffing (4) ──
            if (security != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Event Security Guards",  VendorTypeId = security.Id },
                    new ServiceType { Name = "VIP Bodyguard Service",  VendorTypeId = security.Id },
                    new ServiceType { Name = "Event Hostesses",        VendorTypeId = security.Id },
                    new ServiceType { Name = "Crowd Management",       VendorTypeId = security.Id },
                });

            // ── Makeup & Bridal (4) ──
            if (makeup != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Bridal Makeup",          VendorTypeId = makeup.Id },
                    new ServiceType { Name = "Hair Styling",           VendorTypeId = makeup.Id },
                    new ServiceType { Name = "Group Makeup",           VendorTypeId = makeup.Id },
                    new ServiceType { Name = "Henna Art",              VendorTypeId = makeup.Id },
                });

            // ── Kids Activities (4) ──
            if (kids != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Clown & Entertainer",    VendorTypeId = kids.Id },
                    new ServiceType { Name = "Bounce House Rental",    VendorTypeId = kids.Id },
                    new ServiceType { Name = "Face Painting",          VendorTypeId = kids.Id },
                    new ServiceType { Name = "Kids Game Zone",         VendorTypeId = kids.Id },
                });

            // ── Corporate Services (4) ──
            if (corporate != null)
                serviceTypes.AddRange(new[]
                {
                    new ServiceType { Name = "Keynote Speaker",        VendorTypeId = corporate.Id },
                    new ServiceType { Name = "Translation Services",   VendorTypeId = corporate.Id },
                    new ServiceType { Name = "MC / Event Host",        VendorTypeId = corporate.Id },
                    new ServiceType { Name = "Team Building Activities", VendorTypeId = corporate.Id },
                });

            await context.ServiceTypes.AddRangeAsync(serviceTypes);
            await context.SaveChangesAsync();

            Console.WriteLine("[ServiceTypeSeeding] Completed.");
        }

        // ─────────────────────────────────────────────────────────────────────
        //  SERVICES
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedServicesAsync()
        {
            if (await context.Services.AnyAsync())
                return;

            // ── event types ──
            var wedding = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Wedding");
            var birthday = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Birthday");
            var graduation = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Graduation");
            var corporateEvt = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Corporate Event");
            var conference = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Conference");
            var kidsParty = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Kids Party");
            var exhibition = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Exhibition");
            var concert = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Concert");

            if (wedding == null || birthday == null || graduation == null)
            {
                Console.WriteLine("[ServicesSeeding] EventTypes missing.");
                return;
            }

            // Build allEvents list (include nulls-safe)
            var allEvents = new List<EventType> { wedding, birthday, graduation };
            if (corporateEvt != null) allEvents.Add(corporateEvt);
            if (conference != null) allEvents.Add(conference);
            if (kidsParty != null) allEvents.Add(kidsParty);
            if (exhibition != null) allEvents.Add(exhibition);
            if (concert != null) allEvents.Add(concert);

            // ── service types ──
            var stOpenBuffet = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Open Buffet");
            var stSetMenu = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Set Menu");
            var stLiveCooking = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Live Cooking");
            var stDrinksCorner = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Drinks Corner");
            var stDessertCandyBar = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Dessert & Candy Bar");
            var stCakeDesign = await context.ServiceTypes
                                        .FirstOrDefaultAsync(s => s.Name == "Cake Design" &&
                                                                  s.VendorType.Name == "Catering");

            if (stOpenBuffet == null || stSetMenu == null || stLiveCooking == null ||
                stDrinksCorner == null || stDessertCandyBar == null || stCakeDesign == null)
            {
                Console.WriteLine("[ServicesSeeding] One or more ServiceTypes missing.");
                return;
            }

            // ── vendor lookup helper ──
            async Task<Guid?> VendorId(string email)
            {
                var user = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == email);
                return user?.Id;
            }

            var services = new List<Service>();

            // helper to skip a vendor if not found
            void Add(Guid? vendorId, IEnumerable<Service> vendorServices)
            {
                if (vendorId.HasValue)
                    services.AddRange(vendorServices);
            }

            // ── Abou El Sid ──
            var abouElSidId = await VendorId("catering.abouelsid@placeholder.com");
            Add(abouElSidId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Abou El Sid Full Egyptian Buffet",
            Description = "Buffet of authentic Egyptian classics (kebabs, kofta, mezze, etc.), with dessert station.",
            Price = 30000, VendorId = abouElSidId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 5, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Abou El Sid Set Menu Dinner",
            Description = "Three-course set menu featuring gourmet Egyptian & Mediterranean dishes.",
            Price = 15000, VendorId = abouElSidId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Grill Station",
            Description = "On-site charcoal grill station for fresh kebab, kofta and shish tawook.",
            Price = 18000, VendorId = abouElSidId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert & Candy Bar",
            Description = "Selection of Egyptian desserts and candy (basbousa, konafa, etc.) with candies.",
            Price = 7000, VendorId = abouElSidId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Drinks Corner – Fresh Juices",
            Description = "Cold-pressed juices, mint lemonade and soft drinks station.",
            Price = 4000, VendorId = abouElSidId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday, graduation }
        }
    });

            // ── Zooba ──
            var zoobaId = await VendorId("catering.zooba@placeholder.com");
            Add(zoobaId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Egyptian Street Food Buffet",
            Description = "Buffet of popular street-food dishes (koshary, falafel, hawawshi, etc.).",
            Price = 20000, VendorId = zoobaId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 5, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Zooba Set Menu",
            Description = "Curated set menu highlighting Egyptian favourites in plated courses.",
            Price = 10000, VendorId = zoobaId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Koshary Station",
            Description = "On-site koshary preparation station: rice, lentils, pasta and all toppings served fresh.",
            Price = 12000, VendorId = zoobaId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Drinks Corner – Fresh Juice Bar",
            Description = "Assorted fresh juices, smoothies and mint lemonade drinks station.",
            Price = 3000, VendorId = zoobaId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert & Candy Bar",
            Description = "Traditional desserts and candy bar (basbousa, kahk, assorted candies).",
            Price = 4000, VendorId = zoobaId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        }
    });

            // ── Tabali ──
            var tabaliId = await VendorId("catering.tabali@placeholder.com");
            Add(tabaliId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Mediterranean Buffet",
            Description = "Buffet of Mediterranean favourites (hummus, kebabs, mezze, seafood, etc.).",
            Price = 25000, VendorId = tabaliId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 5, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Premium Set Menu",
            Description = "Plated multi-course meal with gourmet Middle Eastern and international dishes.",
            Price = 15000, VendorId = tabaliId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Grill Station",
            Description = "Open grill station with mixed kebabs and shawarma carved to order.",
            Price = 18000, VendorId = tabaliId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Table",
            Description = "Assortment of pastries and sweets (baklava, kunafa, maamoul) with candy bar.",
            Price = 6000, VendorId = tabaliId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Beverage Station – Coffee & Tea",
            Description = "Hot beverage station with premium Arabic coffee, tea and mint lemonade.",
            Price = 3000, VendorId = tabaliId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Etoile ──
            var etoileId = await VendorId("catering.etoile@placeholder.com");
            Add(etoileId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Classic Buffet Dinner",
            Description = "Elegant buffet with international dishes (meats, salads, sushi, etc.).",
            Price = 30000, VendorId = etoileId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 6, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, birthday, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Gourmet Set Menu",
            Description = "Five-star plated menu with upscale international and oriental cuisine.",
            Price = 20000, VendorId = etoileId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Molecular Station",
            Description = "Interactive live cooking with modern gastronomic techniques.",
            Price = 25000, VendorId = etoileId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 5, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Chocolate Fountain Dessert",
            Description = "Grand dessert station featuring a flowing chocolate fountain and fruit skewers.",
            Price = 10000, VendorId = etoileId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 3, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Premium Beverage Cart",
            Description = "Champagne, mocktails, and premium juices served by a mobile bar cart.",
            Price = 5000, VendorId = etoileId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Cilantro ──
            var cilantroId = await VendorId("catering.cilantro@placeholder.com");
            Add(cilantroId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "International Buffet",
            Description = "Buffet featuring global cuisine (salads, pastas, seafood, etc.) and vegetarian options.",
            Price = 20000, VendorId = cilantroId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 5, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Executive Set Menu",
            Description = "Business-style plated menu with premium dishes (steaks, seafood, etc.).",
            Price = 15000, VendorId = cilantroId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Buffet (Pastries & Cakes)",
            Description = "Extended dessert buffet with cakes, tarts, macarons and signature Cilantro pastries.",
            Price = 8000, VendorId = cilantroId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Signature Cake Design",
            Description = "Custom-designed wedding cake (flavours by choice, artistically decorated).",
            Price = 7000, VendorId = cilantroId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Specialty Coffee Station",
            Description = "Barista service with gourmet coffee, teas and infused coffees.",
            Price = 3000, VendorId = cilantroId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Sequoia ──
            var sequoiaId = await VendorId("catering.sequoia@placeholder.com");
            Add(sequoiaId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Sequoia Luxury Buffet",
            Description = "High-end buffet with gourmet international and seafood dishes.",
            Price = 35000, VendorId = sequoiaId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 6, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Sequoia Set Menu",
            Description = "Refined multi-course menu with premium ingredients (foie gras, lobster, etc.).",
            Price = 20000, VendorId = sequoiaId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Seafood Grill Station",
            Description = "Live grilling station featuring prawns, calamari, and grilled fish fillets.",
            Price = 30000, VendorId = sequoiaId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 5, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Signature Dessert Bar",
            Description = "Premium dessert bar with chocolate fountain, crepes, and specialty cakes.",
            Price = 10000, VendorId = sequoiaId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 3, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Mocktail Bar",
            Description = "Non-alcoholic cocktail bar with fresh fruit drinks and mocktails.",
            Price = 5000, VendorId = sequoiaId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Kazouza ──
            var kazouzaId = await VendorId("catering.kazouza@placeholder.com");
            Add(kazouzaId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Kazouza International Buffet",
            Description = "Buffet with international cuisine (Mediterranean salads, pasta, etc.) and grill station.",
            Price = 22000, VendorId = kazouzaId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 5, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Kazouza Set Menu",
            Description = "Plated menu with premium dishes (steak, fish, vegetarian options).",
            Price = 12000, VendorId = kazouzaId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "BBQ Grill Station",
            Description = "Open barbecue station with kebabs, grilled chicken wings and sausages.",
            Price = 16000, VendorId = kazouzaId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Table",
            Description = "International dessert selection (cakes, pastries, and mini sweets).",
            Price = 6000, VendorId = kazouzaId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Refreshment Corner",
            Description = "Soft drinks, juices and water station with an attendant.",
            Price = 3000, VendorId = kazouzaId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Cook Door ──
            var cookDoorId = await VendorId("catering.cookdoor@placeholder.com");
            Add(cookDoorId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Family Chicken & Pizza Buffet",
            Description = "Buffet featuring Cook Door specials (grilled chicken, shawarma, pizza, fries).",
            Price = 15000, VendorId = cookDoorId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { birthday, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Shawarma & Pizza Set Menu",
            Description = "Meal package with platters of shawarma, pizza and sides for large groups.",
            Price = 10000, VendorId = cookDoorId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Shawarma Station",
            Description = "On-site shawarma carving station with freshly made sauces and salads.",
            Price = 12000, VendorId = cookDoorId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Kids Party Menu",
            Description = "Special menu for children (mini pizzas, chicken fingers, fries, popcorn).",
            Price = 10000, VendorId = cookDoorId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Soft Drinks Corner",
            Description = "Unlimited soft drinks, juices and water station for the event.",
            Price = 3000, VendorId = cookDoorId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert & Candy Bar",
            Description = "Assorted candies and baked treats table (galawati, brownies, jelly).",
            Price = 5000, VendorId = cookDoorId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        }
    });

            // ── Dido's ──
            var didosId = await VendorId("catering.didos@placeholder.com");
            Add(didosId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Deluxe Dessert Buffet",
            Description = "Lavish dessert spread with cakes, tarts, brownies, and chocolate specialties.",
            Price = 8000, VendorId = didosId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Chocolate Fountain Station",
            Description = "Tiered chocolate fountain with fruit skewers and marshmallows for dipping.",
            Price = 6000, VendorId = didosId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Gourmet Pastries Selection",
            Description = "Assorted premium pastries and confectioneries by Dido's chefs.",
            Price = 7000, VendorId = didosId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Customized Celebration Cake",
            Description = "Artisanal cake for weddings/events (design per theme, high-quality ingredients).",
            Price = 5000, VendorId = didosId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 3,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Coffee & Tea Bar",
            Description = "Self-service bar with espresso, cappuccino, tea and specialty coffees.",
            Price = 3000, VendorId = didosId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Willy's Kitchen ──
            var willysId = await VendorId("catering.willyskitchen@placeholder.com");
            Add(willysId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Home-style Buffet",
            Description = "Comfort-food buffet with traditional Egyptian dishes and family favorites.",
            Price = 18000, VendorId = willysId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Set Menu – Family Feast",
            Description = "Hearty plated menu with classic mains (lamb, chicken, rice, etc.).",
            Price = 10000, VendorId = willysId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Live Egyptian Grill",
            Description = "On-site open-flame grill serving kebabs, kofta and shawarma.",
            Price = 14000, VendorId = willysId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 4, LeadTimeRequired = 2,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Table",
            Description = "Traditional sweets buffet (atayef, basbousa, rice pudding, etc.).",
            Price = 5000, VendorId = willysId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Beverage Corner",
            Description = "Soft drinks and juices served in dispensers with self-serve glasses.",
            Price = 3000, VendorId = willysId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Nino's Bakery ──
            var ninosId = await VendorId("happiness@ninosbakeryeg.com");
            Add(ninosId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Signature Wedding Cake",
            Description = "Custom-designed multi-tier wedding cake with premium fillings and decor.",
            Price = 7000, VendorId = ninosId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 7,
            EventTypes = new List<EventType> { wedding }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Custom Celebration Cake",
            Description = "Personalized cake for birthdays or events, hand-crafted designs.",
            Price = 5000, VendorId = ninosId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 5,
            EventTypes = new List<EventType> { birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Platter",
            Description = "Assorted pastries and bites (brownies, cookies, minis) on platters.",
            Price = 4000, VendorId = ninosId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Cupcake & Cookie Tower",
            Description = "Tiered display of decorated cupcakes and gourmet cookies.",
            Price = 3000, VendorId = ninosId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Espresso & Tea Station",
            Description = "Barista service with espresso, cappuccino and selection of teas.",
            Price = 1000, VendorId = ninosId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Cairo Cakes Co ──
            var cairoCakesId = await VendorId("cake.cairocakes@placeholder.com");
            Add(cairoCakesId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Luxury Wedding Cake",
            Description = "Hand-crafted tiered wedding cake with bespoke decoration and flavors.",
            Price = 8000, VendorId = cairoCakesId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 7,
            EventTypes = new List<EventType> { wedding }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Cupcake Tower",
            Description = "Elegant tiered display of gourmet cupcakes and mini pastries.",
            Price = 3000, VendorId = cairoCakesId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Mini Dessert Buffet",
            Description = "Selection of petit fours, cake pops and macarons.",
            Price = 5000, VendorId = cairoCakesId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { birthday, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Chocolate Fountain",
            Description = "Fountain of dark or white chocolate with fruit and marshmallows.",
            Price = 7000, VendorId = cairoCakesId!.Value, ServiceTypeId = stLiveCooking.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Beverage Station (Juices & Tea)",
            Description = "Self-serve station with fresh juices, water and herbal teas.",
            Price = 2000, VendorId = cairoCakesId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Pharaonic Pastries ──
            var pharaonicId = await VendorId("cake.pharaonic@placeholder.com");
            Add(pharaonicId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Oriental Dessert Buffet",
            Description = "Buffet of Egyptian and Middle Eastern sweets (baklava, basbousa, kahk).",
            Price = 6000, VendorId = pharaonicId!.Value, ServiceTypeId = stOpenBuffet.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Traditional Sweets Platter",
            Description = "Assortment platter of kebab, halawa, marzipan and other candies.",
            Price = 5000, VendorId = pharaonicId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Custom Themed Cake",
            Description = "Designer cake with Egyptian motifs (pharaonic, lotus) for special events.",
            Price = 4000, VendorId = pharaonicId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 5,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Baklava Bar",
            Description = "Unlimited baklava station with traditional and modern fillings.",
            Price = 3000, VendorId = pharaonicId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Mint Tea Corner",
            Description = "Serving traditional mint tea and coffee in oriental style cups.",
            Price = 1000, VendorId = pharaonicId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ── Sweet Garden Cairo ──
            var sweetGardenId = await VendorId("cake.sweetgarden@placeholder.com");
            Add(sweetGardenId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Premium Wedding Cake",
            Description = "Bespoke multi-tier cake with floral/garden-themed design.",
            Price = 8000, VendorId = sweetGardenId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 7,
            EventTypes = new List<EventType> { wedding }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Dessert Sampler Table",
            Description = "Variety of mini cakes, macarons and fruit tarts in buffet style.",
            Price = 6000, VendorId = sweetGardenId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Gourmet Cupcake Tower",
            Description = "Tower display of decorated cupcakes in assorted flavours.",
            Price = 4000, VendorId = sweetGardenId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Coffee & Tea Cart",
            Description = "Mobile cart serving espresso, cappuccino and tea alongside pastries.",
            Price = 1000, VendorId = sweetGardenId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Candy Buffet",
            Description = "Table of assorted candies and chocolates, jars for self-serving.",
            Price = 5000, VendorId = sweetGardenId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        }
    });

            // ── Royal Bakeries Cairo ──
            var royalId = await VendorId("cake.royal@placeholder.com");
            Add(royalId, new[]
            {
        new Service {
            Id = Guid.NewGuid(), Name = "Royal Wedding Cake",
            Description = "Exquisite tiered cake with elaborate royal decorations.",
            Price = 10000, VendorId = royalId!.Value, ServiceTypeId = stCakeDesign.Id,
            SetupDuration = 2, LeadTimeRequired = 7,
            EventTypes = new List<EventType> { wedding }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Luxury Pastry Table",
            Description = "Grand pastry buffet (tarts, éclairs, macarons, and premium chocolates).",
            Price = 7000, VendorId = royalId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 3, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Cupcake Assortment",
            Description = "Variety of decorated cupcakes and mini cakes on trays.",
            Price = 4000, VendorId = royalId!.Value, ServiceTypeId = stDessertCandyBar.Id,
            SetupDuration = 1, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Savory Appetizer Platter",
            Description = "Finger foods and appetizers (mini quiches, samosas, bruschetta).",
            Price = 5000, VendorId = royalId!.Value, ServiceTypeId = stSetMenu.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, birthday }
        },
        new Service {
            Id = Guid.NewGuid(), Name = "Mocktail Bar",
            Description = "Selection of non-alcoholic cocktails and fresh juice mocktails.",
            Price = 3000, VendorId = royalId!.Value, ServiceTypeId = stDrinksCorner.Id,
            SetupDuration = 2, LeadTimeRequired = 1,
            EventTypes = new List<EventType> { wedding, graduation }
        }
    });

            // ═══════════════════════════════════════════════════════════════
            //  DECORATION & FLORAL SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stFloral = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Floral Arrangements");
            var stBalloon = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Balloon Decoration");
            var stCenterpiece = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Table Centerpieces");
            var stArch = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Wedding Arch & Backdrop");
            var stFullDecor = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Full Venue Decoration");

            var dreamyId = await VendorId("decor.dreamy@placeholder.com");
            var gardeniaId = await VendorId("decor.gardenia@placeholder.com");
            var elegantId = await VendorId("decor.elegant@placeholder.com");
            var blossomId = await VendorId("decor.blossom@placeholder.com");

            if (stFloral != null && stBalloon != null && stCenterpiece != null && stArch != null && stFullDecor != null)
            {
                Add(dreamyId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Dreamy Floral Arrangements", Description = "Luxury fresh flower arrangements for tables, entrance, and stage.", Price = 12000, VendorId = dreamyId!.Value, ServiceTypeId = stFloral.Id, SetupDuration = 4, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, birthday } },
                    new Service { Id = Guid.NewGuid(), Name = "Dreamy Balloon Canopy", Description = "Ceiling balloon installation and balloon arch at venue entrance.", Price = 6000, VendorId = dreamyId!.Value, ServiceTypeId = stBalloon.Id, SetupDuration = 3, LeadTimeRequired = 1, EventTypes = new List<EventType> { birthday, kidsParty ?? birthday } },
                    new Service { Id = Guid.NewGuid(), Name = "Dreamy Wedding Arch", Description = "Custom floral arch and photo backdrop for the ceremony stage.", Price = 8000, VendorId = dreamyId!.Value, ServiceTypeId = stArch.Id, SetupDuration = 3, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding } },
                });

                Add(gardeniaId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Gardenia Table Centerpieces", Description = "Elegant floral centerpieces for all dining tables.", Price = 9000, VendorId = gardeniaId!.Value, ServiceTypeId = stCenterpiece.Id, SetupDuration = 3, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Gardenia Full Venue Package", Description = "Complete venue decoration including flowers, drapes, and lighting accents.", Price = 25000, VendorId = gardeniaId!.Value, ServiceTypeId = stFullDecor.Id, SetupDuration = 6, LeadTimeRequired = 7, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Gardenia Balloon Arch", Description = "Organic balloon garland arch for stage or photobooth backdrop.", Price = 5000, VendorId = gardeniaId!.Value, ServiceTypeId = stBalloon.Id, SetupDuration = 2, LeadTimeRequired = 1, EventTypes = new List<EventType> { birthday, graduation } },
                });

                Add(elegantId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Elegant Floral Ceiling", Description = "Hanging floral installation and ceiling draping for premium events.", Price = 18000, VendorId = elegantId!.Value, ServiceTypeId = stFloral.Id, SetupDuration = 5, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Elegant Full Venue Decor", Description = "End-to-end event decoration: flowers, drapes, centerpieces, and entrance setup.", Price = 30000, VendorId = elegantId!.Value, ServiceTypeId = stFullDecor.Id, SetupDuration = 8, LeadTimeRequired = 10, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Elegant Bridal Arch", Description = "Premium floral wedding arch with custom theme and color palette.", Price = 10000, VendorId = elegantId!.Value, ServiceTypeId = stArch.Id, SetupDuration = 3, LeadTimeRequired = 7, EventTypes = new List<EventType> { wedding } },
                });

                Add(blossomId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Blossom Table Centerpieces", Description = "Fresh seasonal flower centerpieces with greenery accents.", Price = 7000, VendorId = blossomId!.Value, ServiceTypeId = stCenterpiece.Id, SetupDuration = 3, LeadTimeRequired = 2, EventTypes = new List<EventType> { wedding, birthday } },
                    new Service { Id = Guid.NewGuid(), Name = "Blossom Balloon Decoration", Description = "Balloon columns, ceiling clusters and number/letter balloons.", Price = 4000, VendorId = blossomId!.Value, ServiceTypeId = stBalloon.Id, SetupDuration = 2, LeadTimeRequired = 1, EventTypes = new List<EventType> { birthday } },
                    new Service { Id = Guid.NewGuid(), Name = "Blossom Full Venue Decor", Description = "Budget-friendly full venue decoration with flowers and fabric draping.", Price = 15000, VendorId = blossomId!.Value, ServiceTypeId = stFullDecor.Id, SetupDuration = 5, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding, graduation } },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  SECURITY & STAFFING SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stGuards = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Event Security Guards");
            var stBodyguard = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "VIP Bodyguard Service");
            var stHostesses = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Event Hostesses");
            var stCrowd = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Crowd Management");

            var nileSecId = await VendorId("security.nileevent@placeholder.com");
            var safeguardId = await VendorId("security.safeguard@placeholder.com");
            var egyptGuardsId = await VendorId("security.egyptguards@placeholder.com");
            var pharaohSecId = await VendorId("security.pharaoh@placeholder.com");

            if (stGuards != null && stBodyguard != null && stHostesses != null && stCrowd != null)
            {
                Add(nileSecId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Nile Event Security Team", Description = "Professional uniformed security guards for indoor and outdoor events.", Price = 8000, VendorId = nileSecId!.Value, ServiceTypeId = stGuards.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding, concert ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Nile VIP Bodyguard", Description = "Dedicated close-protection bodyguards for VIPs and celebrities.", Price = 5000, VendorId = nileSecId!.Value, ServiceTypeId = stBodyguard.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Nile Crowd Control", Description = "Trained crowd management specialists for large public events.", Price = 10000, VendorId = nileSecId!.Value, ServiceTypeId = stCrowd.Id, SetupDuration = 2, LeadTimeRequired = 7, EventTypes = new List<EventType> { concert ?? wedding, exhibition ?? graduation } },
                });

                Add(safeguardId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Safeguard Event Hostesses", Description = "Elegantly dressed hostesses for guest reception and registration.", Price = 6000, VendorId = safeguardId!.Value, ServiceTypeId = stHostesses.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding, conference ?? graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Safeguard Security Package", Description = "Full-event security package: guards, CCTV monitoring, and access control.", Price = 12000, VendorId = safeguardId!.Value, ServiceTypeId = stGuards.Id, SetupDuration = 2, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Safeguard VIP Escort", Description = "Discreet VIP escort service for high-profile guests.", Price = 7000, VendorId = safeguardId!.Value, ServiceTypeId = stBodyguard.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding } },
                });

                Add(egyptGuardsId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Egypt Guards Security Team", Description = "Licensed security guards with event management experience.", Price = 7000, VendorId = egyptGuardsId!.Value, ServiceTypeId = stGuards.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, exhibition ?? graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Egypt Guards Crowd Control", Description = "Crowd flow management and emergency response for festivals and concerts.", Price = 9000, VendorId = egyptGuardsId!.Value, ServiceTypeId = stCrowd.Id, SetupDuration = 2, LeadTimeRequired = 5, EventTypes = new List<EventType> { concert ?? wedding, exhibition ?? graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Egypt Guards Hostess Team", Description = "Professional multi-lingual hostesses for international events.", Price = 5000, VendorId = egyptGuardsId!.Value, ServiceTypeId = stHostesses.Id, SetupDuration = 1, LeadTimeRequired = 2, EventTypes = new List<EventType> { corporateEvt ?? wedding, conference ?? graduation } },
                });

                Add(pharaohSecId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Pharaoh Premium Security", Description = "Elite security package with plain-clothes and uniformed officers.", Price = 15000, VendorId = pharaohSecId!.Value, ServiceTypeId = stGuards.Id, SetupDuration = 2, LeadTimeRequired = 7, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Pharaoh VIP Bodyguard", Description = "Executive close-protection service for VIP guests.", Price = 8000, VendorId = pharaohSecId!.Value, ServiceTypeId = stBodyguard.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Pharaoh Event Hostesses", Description = "Bilingual hostesses trained in protocol and VIP guest management.", Price = 6000, VendorId = pharaohSecId!.Value, ServiceTypeId = stHostesses.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, conference ?? graduation } },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  MAKEUP & BRIDAL SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stBridalMakeup = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Bridal Makeup");
            var stHairStyling = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Hair Styling");
            var stGroupMakeup = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Group Makeup");
            var stHenna = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Henna Art");

            var goldenBeautyId = await VendorId("beauty.golden@placeholder.com");
            var bridalBeautyId = await VendorId("beauty.bridal@placeholder.com");
            var divineBeautyId = await VendorId("beauty.divine@placeholder.com");
            var nileWeddingId = await VendorId("beauty.nilewedding@placeholder.com");

            if (stBridalMakeup != null && stHairStyling != null && stGroupMakeup != null && stHenna != null)
            {
                Add(goldenBeautyId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Golden Bridal Makeup", Description = "Full bridal glam with airbrush makeup, contouring, and long-lasting finish.", Price = 3500, VendorId = goldenBeautyId!.Value, ServiceTypeId = stBridalMakeup.Id, SetupDuration = 2, LeadTimeRequired = 7, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Golden Hair Styling", Description = "Bridal updo and blowout styling with extensions if needed.", Price = 1500, VendorId = goldenBeautyId!.Value, ServiceTypeId = stHairStyling.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Golden Henna Night", Description = "Traditional henna art for bride and bridesmaids on henna night.", Price = 2000, VendorId = goldenBeautyId!.Value, ServiceTypeId = stHenna.Id, SetupDuration = 2, LeadTimeRequired = 2, EventTypes = new List<EventType> { wedding } },
                });

                Add(bridalBeautyId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Bridal Studio Full Package", Description = "Complete bridal package: makeup, hair, and on-site touch-up during event.", Price = 5000, VendorId = bridalBeautyId!.Value, ServiceTypeId = stBridalMakeup.Id, SetupDuration = 3, LeadTimeRequired = 14, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Bridesmaid Group Makeup", Description = "Coordinated makeup for up to 6 bridesmaids with matching looks.", Price = 4000, VendorId = bridalBeautyId!.Value, ServiceTypeId = stGroupMakeup.Id, SetupDuration = 3, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Event Hair Styling", Description = "Hair styling for graduation, birthday, and special events.", Price = 1200, VendorId = bridalBeautyId!.Value, ServiceTypeId = stHairStyling.Id, SetupDuration = 1, LeadTimeRequired = 2, EventTypes = new List<EventType> { graduation, birthday } },
                });

                Add(divineBeautyId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Divine Bridal Makeup", Description = "Luxury airbrush bridal makeup with premium imported products.", Price = 4500, VendorId = divineBeautyId!.Value, ServiceTypeId = stBridalMakeup.Id, SetupDuration = 2, LeadTimeRequired = 10, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Divine Group Makeup", Description = "Makeup for parties and events — birthday, graduation, and corporate.", Price = 3000, VendorId = divineBeautyId!.Value, ServiceTypeId = stGroupMakeup.Id, SetupDuration = 2, LeadTimeRequired = 3, EventTypes = new List<EventType> { birthday, graduation, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Divine Henna Art", Description = "Arabic, Indian, and modern henna designs for bride and guests.", Price = 2500, VendorId = divineBeautyId!.Value, ServiceTypeId = stHenna.Id, SetupDuration = 2, LeadTimeRequired = 2, EventTypes = new List<EventType> { wedding } },
                });

                Add(nileWeddingId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Nile Wedding Bridal Glam", Description = "Full-day bridal service: trial session + wedding day makeup and hair.", Price = 6000, VendorId = nileWeddingId!.Value, ServiceTypeId = stBridalMakeup.Id, SetupDuration = 3, LeadTimeRequired = 14, EventTypes = new List<EventType> { wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Nile Hair & Style", Description = "Sophisticated hair styling for weddings, galas, and special occasions.", Price = 2000, VendorId = nileWeddingId!.Value, ServiceTypeId = stHairStyling.Id, SetupDuration = 1, LeadTimeRequired = 3, EventTypes = new List<EventType> { wedding, graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Nile Group Makeup Session", Description = "Group makeup for bridal party or event guests at venue or studio.", Price = 3500, VendorId = nileWeddingId!.Value, ServiceTypeId = stGroupMakeup.Id, SetupDuration = 2, LeadTimeRequired = 5, EventTypes = new List<EventType> { wedding, birthday } },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  KIDS ACTIVITIES SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stClown = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Clown & Entertainer");
            var stBounce = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Bounce House Rental");
            var stFacePaint = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Face Painting");
            var stKidsGame = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Kids Game Zone");

            var kidsKingdomId = await VendorId("kids.kingdom@placeholder.com");
            var happyCastleId = await VendorId("kids.happycastle@placeholder.com");
            var partyJungleId = await VendorId("kids.partyjungle@placeholder.com");
            var funTimeId = await VendorId("kids.funtime@placeholder.com");

            if (stClown != null && stBounce != null && stFacePaint != null && stKidsGame != null)
            {
                var kidsEvents = new List<EventType> { birthday };
                if (kidsParty != null) kidsEvents.Add(kidsParty);

                Add(kidsKingdomId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Kids Kingdom Clown Show", Description = "Funny clown performance with magic tricks, balloon animals, and games.", Price = 2500, VendorId = kidsKingdomId!.Value, ServiceTypeId = stClown.Id, SetupDuration = 1, LeadTimeRequired = 2, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Kids Kingdom Bounce House", Description = "Giant inflatable bounce castle with slides for children of all ages.", Price = 4000, VendorId = kidsKingdomId!.Value, ServiceTypeId = stBounce.Id, SetupDuration = 2, LeadTimeRequired = 3, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Kids Kingdom Face Painting", Description = "Artistic face painting with superhero, princess, and animal themes.", Price = 1500, VendorId = kidsKingdomId!.Value, ServiceTypeId = stFacePaint.Id, SetupDuration = 1, LeadTimeRequired = 1, EventTypes = kidsEvents },
                });

                Add(happyCastleId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Happy Castle Game Zone", Description = "Arcade and carnival-style game stations for kids with prizes.", Price = 5000, VendorId = happyCastleId!.Value, ServiceTypeId = stKidsGame.Id, SetupDuration = 2, LeadTimeRequired = 3, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Happy Castle Bounce House", Description = "Themed inflatable castles (dinosaur, princess, superhero themes).", Price = 3500, VendorId = happyCastleId!.Value, ServiceTypeId = stBounce.Id, SetupDuration = 2, LeadTimeRequired = 2, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Happy Castle Entertainer", Description = "Character entertainer (Mickey, Elsa, Spiderman) for themed birthday parties.", Price = 3000, VendorId = happyCastleId!.Value, ServiceTypeId = stClown.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = kidsEvents },
                });

                Add(partyJungleId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Party Jungle Face Painting", Description = "Professional face and body painting with safe kid-friendly paints.", Price = 2000, VendorId = partyJungleId!.Value, ServiceTypeId = stFacePaint.Id, SetupDuration = 1, LeadTimeRequired = 1, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Party Jungle Game Zone", Description = "Complete kids game zone with obstacle course, ring toss, and ball pit.", Price = 6000, VendorId = partyJungleId!.Value, ServiceTypeId = stKidsGame.Id, SetupDuration = 3, LeadTimeRequired = 3, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Party Jungle Bounce & Slide", Description = "Large inflatable combo unit with bounce area and double slide.", Price = 4500, VendorId = partyJungleId!.Value, ServiceTypeId = stBounce.Id, SetupDuration = 2, LeadTimeRequired = 2, EventTypes = kidsEvents },
                });

                Add(funTimeId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Fun Time Clown & Magic", Description = "Interactive clown show with magic, juggling, and audience participation.", Price = 2800, VendorId = funTimeId!.Value, ServiceTypeId = stClown.Id, SetupDuration = 1, LeadTimeRequired = 2, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Fun Time Game Zone", Description = "Mobile games station including board games, mini golf, and craft tables.", Price = 4500, VendorId = funTimeId!.Value, ServiceTypeId = stKidsGame.Id, SetupDuration = 2, LeadTimeRequired = 2, EventTypes = kidsEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Fun Time Face Painting", Description = "Quick and creative face painting for large groups at parties.", Price = 1800, VendorId = funTimeId!.Value, ServiceTypeId = stFacePaint.Id, SetupDuration = 1, LeadTimeRequired = 1, EventTypes = kidsEvents },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  CORPORATE SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stSpeaker = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Keynote Speaker");
            var stTranslation = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Translation Services");
            var stMC = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "MC / Event Host");
            var stTeamBuild = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Team Building Activities");

            var nileSpeakersId = await VendorId("corp.nilespeakers@placeholder.com");
            var summitTransId = await VendorId("corp.summit@placeholder.com");
            var emcProsId = await VendorId("corp.emcpros@placeholder.com");
            var interworkId = await VendorId("corp.interwork@placeholder.com");

            if (stSpeaker != null && stTranslation != null && stMC != null && stTeamBuild != null)
            {
                var corpEvents = new List<EventType> { graduation };
                if (corporateEvt != null) corpEvents.Add(corporateEvt);
                if (conference != null) corpEvents.Add(conference);

                Add(nileSpeakersId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Nile Keynote Speaker", Description = "Motivational and industry keynote speakers for corporate conferences.", Price = 10000, VendorId = nileSpeakersId!.Value, ServiceTypeId = stSpeaker.Id, SetupDuration = 1, LeadTimeRequired = 14, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Nile Panel Moderator", Description = "Professional panel discussion moderator for seminars and forums.", Price = 6000, VendorId = nileSpeakersId!.Value, ServiceTypeId = stMC.Id, SetupDuration = 1, LeadTimeRequired = 7, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Nile Team Building Workshop", Description = "Structured team-building activities and leadership workshops.", Price = 12000, VendorId = nileSpeakersId!.Value, ServiceTypeId = stTeamBuild.Id, SetupDuration = 2, LeadTimeRequired = 7, EventTypes = corpEvents },
                });

                Add(summitTransId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Summit Simultaneous Translation", Description = "Live Arabic–English simultaneous interpretation with equipment.", Price = 15000, VendorId = summitTransId!.Value, ServiceTypeId = stTranslation.Id, SetupDuration = 2, LeadTimeRequired = 10, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Summit Consecutive Translation", Description = "Professional consecutive translation for meetings and press conferences.", Price = 8000, VendorId = summitTransId!.Value, ServiceTypeId = stTranslation.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Summit Event MC", Description = "Bilingual event host fluent in Arabic and English for formal events.", Price = 5000, VendorId = summitTransId!.Value, ServiceTypeId = stMC.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = corpEvents },
                });

                Add(emcProsId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "EMC Pro Event Host", Description = "Charismatic MC for corporate events, galas, and award ceremonies.", Price = 7000, VendorId = emcProsId!.Value, ServiceTypeId = stMC.Id, SetupDuration = 1, LeadTimeRequired = 7, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "EMC Corporate Speaker", Description = "Expert corporate speakers covering leadership, innovation, and strategy.", Price = 12000, VendorId = emcProsId!.Value, ServiceTypeId = stSpeaker.Id, SetupDuration = 1, LeadTimeRequired = 14, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "EMC Team Building Games", Description = "Fun and competitive team-building games for corporate outings.", Price = 9000, VendorId = emcProsId!.Value, ServiceTypeId = stTeamBuild.Id, SetupDuration = 2, LeadTimeRequired = 5, EventTypes = corpEvents },
                });

                Add(interworkId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Interwork Translation Booth", Description = "Full translation booth setup with certified interpreters for large events.", Price = 20000, VendorId = interworkId!.Value, ServiceTypeId = stTranslation.Id, SetupDuration = 3, LeadTimeRequired = 14, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Interwork Event MC", Description = "Professional bilingual hosts for conferences and corporate ceremonies.", Price = 6000, VendorId = interworkId!.Value, ServiceTypeId = stMC.Id, SetupDuration = 1, LeadTimeRequired = 5, EventTypes = corpEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Interwork Team Building", Description = "Creative workshops and problem-solving activities for corporate teams.", Price = 10000, VendorId = interworkId!.Value, ServiceTypeId = stTeamBuild.Id, SetupDuration = 2, LeadTimeRequired = 7, EventTypes = corpEvents },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  COWORKING SPACE SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stPrivateOffice = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Private Office");
            var stMeetingRoom = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Meeting Room");
            var stHotDesk = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Hot Desk");
            var stEventSpace = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Event Space Rental");

            var consoleyaId = await VendorId("hello@consoleya.com");
            var cubeSpaceId = await VendorId("admin@cubespaceeg.com");
            var kmtHouseId = await VendorId("info@kmthouse.com");
            var theDistrictId = await VendorId("hello@thedistrict-eg.com");

            if (stPrivateOffice != null && stMeetingRoom != null && stHotDesk != null && stEventSpace != null)
            {
                var coworkEvents = new List<EventType> { graduation };
                if (corporateEvt != null) coworkEvents.Add(corporateEvt);
                if (conference != null) coworkEvents.Add(conference);

                Add(consoleyaId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Consoleya Private Office", Description = "Fully equipped private office for teams of up to 6 people.", Price = 5000, VendorId = consoleyaId!.Value, ServiceTypeId = stPrivateOffice.Id, SetupDuration = 0, LeadTimeRequired = 1, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Consoleya Meeting Room", Description = "Modern meeting room with projector, whiteboard, and high-speed WiFi.", Price = 2000, VendorId = consoleyaId!.Value, ServiceTypeId = stMeetingRoom.Id, SetupDuration = 0, LeadTimeRequired = 1, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Consoleya Hot Desk", Description = "Flexible daily hot-desk access with WiFi and lounge facilities.", Price = 500, VendorId = consoleyaId!.Value, ServiceTypeId = stHotDesk.Id, SetupDuration = 0, LeadTimeRequired = 0, EventTypes = coworkEvents },
                });

                Add(cubeSpaceId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Cube Space Event Hall", Description = "Open event hall for workshops, networking nights, and product launches.", Price = 8000, VendorId = cubeSpaceId!.Value, ServiceTypeId = stEventSpace.Id, SetupDuration = 2, LeadTimeRequired = 3, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Cube Space Meeting Room", Description = "AV-equipped meeting room for 10–20 participants.", Price = 2500, VendorId = cubeSpaceId!.Value, ServiceTypeId = stMeetingRoom.Id, SetupDuration = 0, LeadTimeRequired = 1, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Cube Space Private Office", Description = "Dedicated private office with 24/7 access and printing services.", Price = 6000, VendorId = cubeSpaceId!.Value, ServiceTypeId = stPrivateOffice.Id, SetupDuration = 0, LeadTimeRequired = 1, EventTypes = coworkEvents },
                });

                Add(kmtHouseId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "KMT House Hot Desk", Description = "Creative open-desk workspace in Garden City with café and rooftop.", Price = 600, VendorId = kmtHouseId!.Value, ServiceTypeId = stHotDesk.Id, SetupDuration = 0, LeadTimeRequired = 0, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "KMT House Event Space", Description = "Heritage villa event space for cultural events and brand activations.", Price = 10000, VendorId = kmtHouseId!.Value, ServiceTypeId = stEventSpace.Id, SetupDuration = 3, LeadTimeRequired = 5, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "KMT House Meeting Room", Description = "Intimate board room for executive meetings and workshops.", Price = 3000, VendorId = kmtHouseId!.Value, ServiceTypeId = stMeetingRoom.Id, SetupDuration = 0, LeadTimeRequired = 1, EventTypes = coworkEvents },
                });

                Add(theDistrictId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "The District Private Suite", Description = "Premium private office suite for startups and remote teams.", Price = 7000, VendorId = theDistrictId!.Value, ServiceTypeId = stPrivateOffice.Id, SetupDuration = 0, LeadTimeRequired = 2, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "The District Event Space", Description = "Modern event space for product launches, panels, and pitch events.", Price = 9000, VendorId = theDistrictId!.Value, ServiceTypeId = stEventSpace.Id, SetupDuration = 2, LeadTimeRequired = 3, EventTypes = coworkEvents },
                    new Service { Id = Guid.NewGuid(), Name = "The District Hot Desk", Description = "Day-pass hot desking with community access and high-speed internet.", Price = 450, VendorId = theDistrictId!.Value, ServiceTypeId = stHotDesk.Id, SetupDuration = 0, LeadTimeRequired = 0, EventTypes = coworkEvents },
                });
            }

            // ═══════════════════════════════════════════════════════════════
            //  PRODUCTION SERVICES
            // ═══════════════════════════════════════════════════════════════
            var stEventMgmt = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Event Management");
            var stStageProd = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Stage Production");
            var stExhibition = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Exhibition Setup");
            var stConfProd = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Conference Production");

            var eventPlanetId = await VendorId("info@eventplaneteg.com");
            var leapfrogId = await VendorId("clientservice@leapfrog.com.eg");
            var septemId = await VendorId("info@septemevents.com");
            var visionEventsId = await VendorId("info@vision-events.net");

            if (stEventMgmt != null && stStageProd != null && stExhibition != null && stConfProd != null)
            {
                var prodEvents = new List<EventType> { wedding, graduation };
                if (corporateEvt != null) prodEvents.Add(corporateEvt);
                if (conference != null) prodEvents.Add(conference);
                if (exhibition != null) prodEvents.Add(exhibition);
                if (concert != null) prodEvents.Add(concert);

                Add(eventPlanetId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Event Planet Full Management", Description = "End-to-end event management from planning to execution.", Price = 30000, VendorId = eventPlanetId!.Value, ServiceTypeId = stEventMgmt.Id, SetupDuration = 0, LeadTimeRequired = 30, EventTypes = prodEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Event Planet Stage Production", Description = "Full stage setup including truss, lighting rigs, LED walls, and sound.", Price = 25000, VendorId = eventPlanetId!.Value, ServiceTypeId = stStageProd.Id, SetupDuration = 8, LeadTimeRequired = 14, EventTypes = prodEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Event Planet Conference Setup", Description = "Conference production: AV, stage, registration area, and signage.", Price = 20000, VendorId = eventPlanetId!.Value, ServiceTypeId = stConfProd.Id, SetupDuration = 6, LeadTimeRequired = 14, EventTypes = new List<EventType> { conference ?? graduation, corporateEvt ?? wedding } },
                });

                Add(leapfrogId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "LEAPFROG Exhibition Stand", Description = "Custom-designed exhibition stands and booth fabrication.", Price = 35000, VendorId = leapfrogId!.Value, ServiceTypeId = stExhibition.Id, SetupDuration = 10, LeadTimeRequired = 21, EventTypes = new List<EventType> { exhibition ?? graduation, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "LEAPFROG Stage Production", Description = "Mega-stage production for concerts, festivals, and large corporate events.", Price = 40000, VendorId = leapfrogId!.Value, ServiceTypeId = stStageProd.Id, SetupDuration = 12, LeadTimeRequired = 21, EventTypes = prodEvents },
                    new Service { Id = Guid.NewGuid(), Name = "LEAPFROG Event Management", Description = "Full-service event production management for large-scale events.", Price = 50000, VendorId = leapfrogId!.Value, ServiceTypeId = stEventMgmt.Id, SetupDuration = 0, LeadTimeRequired = 45, EventTypes = prodEvents },
                });

                Add(septemId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Septem Conference Production", Description = "Complete AV and staging production for conferences and summits.", Price = 18000, VendorId = septemId!.Value, ServiceTypeId = stConfProd.Id, SetupDuration = 5, LeadTimeRequired = 10, EventTypes = new List<EventType> { conference ?? graduation, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Septem Exhibition Setup", Description = "Modular exhibition booth design and setup for trade shows.", Price = 22000, VendorId = septemId!.Value, ServiceTypeId = stExhibition.Id, SetupDuration = 8, LeadTimeRequired = 14, EventTypes = new List<EventType> { exhibition ?? graduation } },
                    new Service { Id = Guid.NewGuid(), Name = "Septem Event Management", Description = "Turnkey event management including logistics, vendors, and on-site ops.", Price = 25000, VendorId = septemId!.Value, ServiceTypeId = stEventMgmt.Id, SetupDuration = 0, LeadTimeRequired = 30, EventTypes = prodEvents },
                });

                Add(visionEventsId, new[]
                {
                    new Service { Id = Guid.NewGuid(), Name = "Vision Stage Production", Description = "Professional stage, truss, and lighting production for events.", Price = 20000, VendorId = visionEventsId!.Value, ServiceTypeId = stStageProd.Id, SetupDuration = 6, LeadTimeRequired = 10, EventTypes = prodEvents },
                    new Service { Id = Guid.NewGuid(), Name = "Vision Exhibition Stand", Description = "Branded exhibition stand design, printing, and installation.", Price = 15000, VendorId = visionEventsId!.Value, ServiceTypeId = stExhibition.Id, SetupDuration = 6, LeadTimeRequired = 14, EventTypes = new List<EventType> { exhibition ?? graduation, corporateEvt ?? wedding } },
                    new Service { Id = Guid.NewGuid(), Name = "Vision Full Event Management", Description = "Comprehensive event planning and management for corporate and social events.", Price = 28000, VendorId = visionEventsId!.Value, ServiceTypeId = stEventMgmt.Id, SetupDuration = 0, LeadTimeRequired = 21, EventTypes = prodEvents },
                });
            }

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();

            Console.WriteLine($"[ServicesSeeding] Completed. {services.Count} services seeded.");
        }
        // ─────────────────────────────────────────────────────────────────────
        //  EVENT
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedEventAsync()
        {
            Console.WriteLine("[EventSeeding] Starting event seeding...");

            var user = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");
            var eventType = context.EventTypes.FirstOrDefault(x => x.Name == "Wedding");
            var vendor = context.ApplicationUsers.FirstOrDefault(v => v.Email == "vendor@example.com");

            if (user == null) { Console.WriteLine("[EventSeeding] ❌ user not found"); return; }
            if (eventType == null) { Console.WriteLine("[EventSeeding] ❌ eventType not found"); return; }
            if (vendor == null) { Console.WriteLine("[EventSeeding] ❌ vendor not found"); return; }

            var photoService = context.Services.FirstOrDefault(s => s.Name == "Candy Buffet");
            var decorService = context.Services.FirstOrDefault(s => s.Name == "Luxury Pastry Table");

            if (photoService == null) { Console.WriteLine("[EventSeeding] ❌ photoService not found"); return; }
            if (decorService == null) { Console.WriteLine("[EventSeeding] ❌ decorService not found"); return; }

            if (context.Events.Any(e => e.Title == "Luxury Wedding Cairo 2026"))
            {
                Console.WriteLine("[EventSeeding] ⚠️ Event already exists, skipping.");
                return;
            }

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
                EventItems = new List<EventItem>
                {
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,  Price = photoService.Price, Quantity = 1, ItemStatus = "Approved", RejectionReason = null },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,       Price = photoService.Price,    Quantity = 1, ItemStatus = "Pending"  },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = decorService.Id,    Price = decorService.Price,    Quantity = 1, ItemStatus = "Pending"  }
                }
            };
            var newEvent2 = new Event
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
                EventItems = new List<EventItem>
                {
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,  Price = photoService.Price, Quantity = 1, ItemStatus = "Approved", RejectionReason = null },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,       Price = photoService.Price,    Quantity = 1, ItemStatus = "Pending"  },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = decorService.Id,    Price = decorService.Price,    Quantity = 1, ItemStatus = "Pending"  }
                }
            };
            var newEvent3 = new Event
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
                EventItems = new List<EventItem>
                {
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,  Price = photoService.Price, Quantity = 1, ItemStatus = "Approved", RejectionReason = null },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = photoService.Id,       Price = photoService.Price,    Quantity = 1, ItemStatus = "Pending"  },
                    new EventItem { Id = Guid.NewGuid(), ServiceId = decorService.Id,    Price = decorService.Price,    Quantity = 1, ItemStatus = "Pending"  }
                }
            };
            Console.WriteLine("[EventSeeding] Event created successfully.");

            await context.Events.AddRangeAsync(newEvent, newEvent2, newEvent3);
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
            var serviceIds = context.Services.Where(s => s.VendorId == vendor.UserId).Select(s => s.Id).Take(3).ToList();
            var packages = new List<Package>
            {
                new Package { Id = Guid.NewGuid(), Name = "Basic Wedding Package",   Description = "Basic wedding package",   Price = 10000, Discount = 10, VendorId = vendor.UserId, ServiceIds = serviceIds }
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

            var vendor = context.ApplicationUsers.FirstOrDefault(u => u.Email == "vendor@example.com");
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
            var user = context.ApplicationUsers.FirstOrDefault(u => u.Email == "customer@example.com");
            var existingEvent = context.Events.FirstOrDefault(e => e.Title == "Luxury Wedding Cairo 2026");

            if (user == null || existingEvent == null) return;
            if (context.Orders.Any(o => o.EventId == existingEvent.Id)) return;

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
                ShippingAddress = new Address { City = "Cairo", State = "Giza", Street = "Pyramids Road" },
                CreatedAt = DateTime.UtcNow
            };

            context.Orders.Add(order);
            await context.SaveChangesAsync();
        }

        // ─────────────────────────────────────────────────────────────────────
        //  CILANTRO ACCOUNT SEEDING
        // ─────────────────────────────────────────────────────────────────────
        private async Task SeedCilantroDataAsync()
        {
            var cilantroEmail = "catering.cilantro@placeholder.com";
            var cilantroUser = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == cilantroEmail);
            var customer = await context.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == "customer@example.com");

            if (cilantroUser == null)
            {
                Console.WriteLine("[CilantroSeeding] Cilantro user not found.");
                return;
            }

            var cilantroVendor = await context.Vendors.FirstOrDefaultAsync(v => v.UserId == cilantroUser.Id);

            if (cilantroVendor != null)
            {
                // Profile
                cilantroVendor.BusinessName = "Cilantro Catering Excellence";
                cilantroVendor.Description = "Premium catering services with an exquisite touch. Specializing in high-end events, luxury dining experiences, corporate gatherings, and unforgettable weddings.";
                cilantroVendor.YearsInBusiness = 15;
                cilantroVendor.IsVerified = true;
                cilantroVendor.PortfolioLink = "https://cilantrocatering.example.com";
                cilantroVendor.ProfilePicture = "https://example.com/cilantro-profile.jpg";
            }

            // Reference Data
            var wedding = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Wedding");
            var birthday = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Birthday");
            var graduation = await context.EventTypes.FirstOrDefaultAsync(e => e.Name == "Graduation");

            var stOpenBuffet = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Open Buffet");
            var stDrinksCorner = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Drinks Corner");
            var stSetMenu = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Set Menu");
            var stLiveCooking = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Live Cooking");
            var stDessert = await context.ServiceTypes.FirstOrDefaultAsync(s => s.Name == "Dessert & Candy Bar");

            var allEvents = new List<EventType>();
            if (wedding != null) allEvents.Add(wedding);
            if (birthday != null) allEvents.Add(birthday);
            if (graduation != null) allEvents.Add(graduation);

            // Services
            if (!await context.Services.AnyAsync(s => s.VendorId == cilantroUser.Id && s.Name.Contains("Cilantro")))
            {
                var services = new List<Service>();

                if (stOpenBuffet != null)
                {
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Royal Open Buffet",
                        Description = "A luxurious open buffet featuring international cuisines and delicate desserts.",
                        Price = 45000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stOpenBuffet.Id,
                        SetupDuration = 6,
                        LeadTimeRequired = 7,
                        EventTypes = allEvents.ToList()
                    });
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Corporate Buffet",
                        Description = "Professional setup for corporate events including finger foods, appetizers, and light meals.",
                        Price = 25000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stOpenBuffet.Id,
                        SetupDuration = 3,
                        LeadTimeRequired = 5,
                        EventTypes = allEvents.ToList()
                    });
                }

                if (stDrinksCorner != null)
                {
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Premium Drinks Corner",
                        Description = "Fresh juices, premium coffee, and mocktails served by professional baristas.",
                        Price = 12000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stDrinksCorner.Id,
                        SetupDuration = 3,
                        LeadTimeRequired = 3,
                        EventTypes = allEvents.ToList()
                    });
                }

                if (stSetMenu != null)
                {
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Gourmet Set Menu",
                        Description = "Three-course gourmet meal perfect for intimate gatherings and upscale weddings.",
                        Price = 30000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stSetMenu.Id,
                        SetupDuration = 4,
                        LeadTimeRequired = 10,
                        EventTypes = wedding != null ? new List<EventType> { wedding } : new List<EventType>()
                    });
                }

                if (stLiveCooking != null)
                {
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Live Pasta & Grill Station",
                        Description = "Interactive live cooking stations featuring fresh pasta, premium steaks, and seafood.",
                        Price = 35000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stLiveCooking.Id,
                        SetupDuration = 5,
                        LeadTimeRequired = 14,
                        EventTypes = allEvents.ToList()
                    });
                }

                if (stDessert != null)
                {
                    services.Add(new Service
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Dreamy Dessert Bar",
                        Description = "An elaborate dessert and candy bar featuring chocolate fountains, macarons, and custom cakes.",
                        Price = 18000,
                        VendorId = cilantroUser.Id,
                        ServiceTypeId = stDessert.Id,
                        SetupDuration = 3,
                        LeadTimeRequired = 7,
                        EventTypes = allEvents.ToList()
                    });
                }

                if (services.Any())
                {
                    await context.Services.AddRangeAsync(services);
                    await context.SaveChangesAsync();
                }
            }

            // Packages
            if (!await context.Packages.AnyAsync(p => p.VendorId == cilantroUser.Id && p.Name.Contains("Cilantro")))
            {
                var cilantroServices = await context.Services.Where(s => s.VendorId == cilantroUser.Id).ToListAsync();

                var buffetServices = cilantroServices.Where(s => s.Name.Contains("Buffet") || s.Name.Contains("Drinks")).Select(s => s.Id).ToList();
                var premiumServices = cilantroServices.Select(s => s.Id).ToList();
                var corporateServices = cilantroServices.Where(s => s.Name.Contains("Corporate") || s.Name.Contains("Drinks")).Select(s => s.Id).ToList();

                var packages = new List<Package>();

                if (buffetServices.Any())
                {
                    packages.Add(new Package
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Platinum Wedding Package",
                        Description = "Complete catering solution including our royal open buffet and drinks corner.",
                        Price = 50000,
                        Discount = 15,
                        VendorId = cilantroUser.Id,
                        ServiceIds = buffetServices
                    });
                }

                if (premiumServices.Any())
                {
                    packages.Add(new Package
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Ultimate VIP Experience",
                        Description = "Everything we have to offer: Buffet, Live Cooking, Drinks, and Dessert Bar.",
                        Price = 90000,
                        Discount = 25,
                        VendorId = cilantroUser.Id,
                        ServiceIds = premiumServices
                    });
                }

                if (corporateServices.Any())
                {
                    packages.Add(new Package
                    {
                        Id = Guid.NewGuid(),
                        Name = "Cilantro Business Elite Package",
                        Description = "Perfect for corporate retreats and large business meetings.",
                        Price = 32000,
                        Discount = 10,
                        VendorId = cilantroUser.Id,
                        ServiceIds = corporateServices
                    });
                }

                if (packages.Any())
                {
                    await context.Packages.AddRangeAsync(packages);
                    await context.SaveChangesAsync();
                }
            }

            // Rating & Review
            var cilantroServiceIds = await context.Services.Where(s => s.VendorId == cilantroUser.Id).Select(s => s.Id).ToListAsync();
            if (customer != null && cilantroServiceIds.Any() && !await context.ServiceRatings.AnyAsync(r => r.UserId == customer.Id && cilantroServiceIds.Contains(r.ServiceId)))
            {
                var ratings = new List<ServiceRating>
                {
                    new ServiceRating { Id = Guid.NewGuid(), ServiceId = cilantroServiceIds.First(), UserId = customer.Id, Rating = 4.8m, Review = "Exceptional service! The food was delicious and the presentation was breathtaking." },
                    new ServiceRating { Id = Guid.NewGuid(), ServiceId = cilantroServiceIds.Last(), UserId = customer.Id, Rating = 5.0m, Review = "Absolutely amazing! The staff was so professional and the live cooking station was a huge hit." },
                    new ServiceRating { Id = Guid.NewGuid(), ServiceId = cilantroServiceIds[cilantroServiceIds.Count / 2], UserId = customer.Id, Rating = 4.5m, Review = "Great variety of drinks. Only minor issue was they arrived slightly late, but they made up for it." },
                    new ServiceRating { Id = Guid.NewGuid(), ServiceId = cilantroServiceIds.First(), UserId = customer.Id, Rating = 4.9m, Review = "Highly recommend for any major event. Best catering we've ever hired." }
                };
                await context.ServiceRatings.AddRangeAsync(ratings);
            }


            // Chat (Conversation & Messages)
            if (customer != null && !await context.Conversations.AnyAsync(c => c.User1Id == customer.Id && c.User2Id == cilantroUser.Id))
            {
                var conv1 = Conversation.Create(customer.Id, cilantroUser.Id);
                await context.Conversations.AddAsync(conv1);
                await context.SaveChangesAsync();

                var messages = new List<Message>
                {
                    Message.Create(conv1.Id, customer.Id, cilantroUser.Id, "Hello, we are interested in your royal buffet for our upcoming wedding."),
                    Message.Create(conv1.Id, cilantroUser.Id, customer.Id, "Welcome! We would be delighted to cater for your special day. Could you share the date and expected number of guests?"),
                    Message.Create(conv1.Id, customer.Id, cilantroUser.Id, "We are planning for October 15th, around 200 guests."),
                    Message.Create(conv1.Id, cilantroUser.Id, customer.Id, "Perfect! For 200 guests, I would also recommend looking into our Ultimate VIP package which includes the live cooking station."),
                    Message.Create(conv1.Id, customer.Id, cilantroUser.Id, "That sounds interesting, do you have a detailed menu for that?"),
                    Message.Create(conv1.Id, cilantroUser.Id, customer.Id, "Yes, absolutely! I will send over the brochure shortly. Will there be any dietary restrictions?"),
                    Message.Create(conv1.Id, customer.Id, cilantroUser.Id, "A few vegan guests, maybe 10 people."),
                    Message.Create(conv1.Id, cilantroUser.Id, customer.Id, "Not a problem at all, we can customize a dedicated section for them.")
                };

                await context.Messages.AddRangeAsync(messages);
            }

            // Notifications
            if (!await context.Notifications.AnyAsync(n => n.UserId == cilantroUser.Id && n.Title.Contains("Cilantro")))
            {
                var notificationTypes = Enum.GetValues(typeof(NotificationType)).Cast<NotificationType>();
                var notifications = new List<Notification>();
                foreach (var type in notificationTypes)
                {
                    notifications.Add(new Notification
                    {
                        Id = Guid.NewGuid(),
                        UserId = cilantroUser.Id,
                        Type = type,
                        Title = $"System Update: {type}",
                        Message = $"This is an automated system event regarding {type}. Please check your dashboard for more details.",
                        CreatedAt = DateTime.UtcNow.AddHours(-new Random().Next(1, 48)),
                        IsRead = new Random().Next(0, 2) == 1
                    });
                }

                // Add some duplicate specific ones for more volume
                notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = cilantroUser.Id, Type = NotificationType.ORDER_PLACED, Title = "New Corporate Order", Message = "A new corporate event has been booked for next week.", CreatedAt = DateTime.UtcNow.AddMinutes(-30), IsRead = false });
                notifications.Add(new Notification { Id = Guid.NewGuid(), UserId = cilantroUser.Id, Type = NotificationType.ORDER_COMPLETED, Title = "Event Concluded", Message = "The wedding at Grand Hotel has been marked as completed. Please review.", CreatedAt = DateTime.UtcNow.AddDays(-1), IsRead = true });

                await context.Notifications.AddRangeAsync(notifications);
            }

            await context.SaveChangesAsync();
            Console.WriteLine("[CilantroSeeding] Completed with expanded data.");
        }
    }
}