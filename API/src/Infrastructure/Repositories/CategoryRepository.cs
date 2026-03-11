
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Entities;
using Domain.Contracts;


namespace Infrastructure.Repositories
{
    public class CategoryRepository(ApplicationDbContext dbContext) : ICategoryRepository
    {
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            var categories = await dbContext.Categories.ToListAsync();
            return categories;
        }


        public async Task<Category?> GetCategoryByIdAsync(Guid id)
        {
            var category = await dbContext.Categories.FindAsync(id);
            return category;
        }

        public async Task AddCategoryAsync(Category category)
        {
            await dbContext.Categories.AddAsync(category);
            await dbContext.SaveChangesAsync();
        }


        public async Task DeleteCategoryAsync(Category category)
        {
          
                dbContext.Categories.Remove(category);
                await dbContext.SaveChangesAsync();
            
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            dbContext.Categories.Update(category);
            await dbContext.SaveChangesAsync();
        }
    }
}
