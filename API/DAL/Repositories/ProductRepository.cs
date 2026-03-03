using DAL.Context;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using DAL.Repositories.Contracts;
namespace DAL.Repositories
{
    public class ProductRepository(ApplicationDbContext dbContext) : IProductRepository
    {


        public async Task CreateProduct(Product product)
        {
            dbContext.Products.Add(product);
            await dbContext.SaveChangesAsync();
        }


        public async Task DeleteProduct(Product product)
        {
            dbContext.Products.Remove(product);
            await dbContext.SaveChangesAsync();

        }

        public async Task<Product> GetProductById(Guid id)
        {
            return await dbContext.Products.FindAsync(id);
        }


        public async Task<List<Product>> GetAllProducts()
        {
            return await dbContext.Products.ToListAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            dbContext.Products.Update(product);
            await dbContext.SaveChangesAsync();
        }

    }
}
