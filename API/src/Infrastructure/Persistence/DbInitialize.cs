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

            var furniture = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Furniture & Setup");
            var media = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Media");
            var catering = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Catering");
            var cakeDesign = catering;
            var entertainment = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Entertainment");
            var printing = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Printing");
            var transportation = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Transportation");
            var venue = await context.VendorTypes.FirstOrDefaultAsync(v => v.Name == "Venue");

            if (furniture == null || media == null || catering == null || cakeDesign == null ||
                entertainment == null || printing == null || transportation == null || venue == null)
            {
                Console.WriteLine("[ServiceTypeSeeding] One or more VendorTypes missing.");
                return;
            }

            var serviceTypes = new List<ServiceType>
    {
        // Furniture & Setup
        new ServiceType { Name = "Chairs Rental",        VendorTypeId = furniture.Id },
        new ServiceType { Name = "Tables Rental",        VendorTypeId = furniture.Id },
        new ServiceType { Name = "Stage Setup",          VendorTypeId = furniture.Id },
        new ServiceType { Name = "Lighting System Setup",VendorTypeId = furniture.Id },
        new ServiceType { Name = "Sound System Setup",   VendorTypeId = furniture.Id },
        new ServiceType { Name = "LED Screens",          VendorTypeId = furniture.Id },

        // Media
        new ServiceType { Name = "Photographer",         VendorTypeId = media.Id },
        new ServiceType { Name = "Videographer",         VendorTypeId = media.Id },
        new ServiceType { Name = "Drone Videography",    VendorTypeId = media.Id },
        new ServiceType { Name = "Photobooth",           VendorTypeId = media.Id },

        // Catering
        new ServiceType { Name = "Open Buffet",          VendorTypeId = catering.Id },
        new ServiceType { Name = "Set Menu",             VendorTypeId = catering.Id },
        new ServiceType { Name = "Live Cooking",         VendorTypeId = catering.Id },
        new ServiceType { Name = "Drinks Corner",        VendorTypeId = catering.Id },
        new ServiceType { Name = "Dessert & Candy Bar",  VendorTypeId = catering.Id },

        // Cake Design (maps to both Catering and Cake Design vendor type)
        new ServiceType { Name = "Cake Design",          VendorTypeId = catering.Id },
        new ServiceType { Name = "Cake Design",          VendorTypeId = cakeDesign.Id },

        // Venue
        new ServiceType { Name = "Indoor Venue",         VendorTypeId = venue.Id },
        new ServiceType { Name = "Outdoor Venue",        VendorTypeId = venue.Id },

        // Entertainment
        new ServiceType { Name = "DJ",                   VendorTypeId = entertainment.Id },

        // Printing
        new ServiceType { Name = "Printed Invitations",  VendorTypeId = printing.Id },
        new ServiceType { Name = "Digital Invitations",  VendorTypeId = printing.Id },
        new ServiceType { Name = "Banners",              VendorTypeId = printing.Id },
        new ServiceType { Name = "Customized Cards",     VendorTypeId = printing.Id },

        // Transportation
        new ServiceType { Name = "Luxury Transport",     VendorTypeId = transportation.Id },
        new ServiceType { Name = "Limousine",            VendorTypeId = transportation.Id },
        new ServiceType { Name = "Shuttle Transport",    VendorTypeId = transportation.Id },
    };

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

            if (wedding == null || birthday == null || graduation == null)
            {
                Console.WriteLine("[ServicesSeeding] EventTypes missing.");
                return;
            }

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

            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();

            Console.WriteLine($"[ServicesSeeding] Completed. {services.Count} services seeded.");
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
                new Package { Id = Guid.NewGuid(), Name = "Basic Wedding Package",   Description = "Basic wedding package",   Price = 10000, Discount = 10, VendorId = vendor.UserId, ServiceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() } },
                new Package { Id = Guid.NewGuid(), Name = "Premium Wedding Package", Description = "Premium wedding package", Price = 25000, Discount = 15, VendorId = vendor.UserId, ServiceIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() } }
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