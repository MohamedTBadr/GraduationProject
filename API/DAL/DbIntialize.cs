using DAL.Context;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace DAL
{
    public class DbIntialize(ApplicationDbContext context) : IDbIntialize
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
                await ProductSeeding();

            
            }
            catch (Exception E)
            {
                Console.WriteLine($"Error Occurred during seeding: {E.Message}");
            }

        }

        private async Task ProductSeeding()
        {
            if (!context.Set<Product>().Any())
            {
                var data = await File.ReadAllTextAsync(@"../Infrastructure\Presistence\Seeding\brands.json");

                var Products = JsonSerializer.Deserialize<List<Product>>(data);

                if (Products is not null && Products.Any())
                {
                    context.Set<Product>().AddRange(Products);
                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

