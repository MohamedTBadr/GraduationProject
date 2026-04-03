
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;
using Domain.Entities;
using Domain.Contracts;
using Polly.Registry;
using Polly;


namespace Infrastructure.Repositories
{
    public class CategoryRepository(ApplicationDbContext dbContext, ResiliencePipelineProvider<string> pipelineProvider) : ICategoryRepository
    {
        private readonly ResiliencePipeline _pipeline = pipelineProvider.GetPipeline("db-pipeline");

        public async Task<List<Category>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = await dbContext.Categories.ToListAsync(cancellationToken);
            return categories;
        }


        public async Task<Category?> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await _pipeline.ExecuteAsync(async token => await dbContext.Categories.FindAsync([id], token), cancellationToken);
            return category;
        }

        public async Task AddCategoryAsync(Category category, CancellationToken cancellationToken)
        {
            await _pipeline.ExecuteAsync(async token => {

            await dbContext.Categories.AddAsync(category, token);
            await dbContext.SaveChangesAsync(token);
                    }, cancellationToken);
        }


        public async Task DeleteCategoryAsync(Category category, CancellationToken cancellationToken)
        {
          
                dbContext.Categories.Remove(category);
                await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
            
        }

        public async Task UpdateCategoryAsync(Category category, CancellationToken cancellationToken)
        {
            dbContext.Categories.Update(category);
            await _pipeline.ExecuteAsync(async token => await dbContext.SaveChangesAsync(token), cancellationToken);
        }
    }
}
