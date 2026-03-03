using DAL.Repositories;
using System;
using System.Collections.Generic;
using System.Text;
using DAL.Entities;
using BLL.DTOs.CategoryDTOs;
using DAL.Repositories.Contracts;
using AutoMapper;
using BLL.Services.Interfaces;
namespace BLL.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper) : ICategoryService
    {
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await categoryRepository.GetAllCategoriesAsync();
        }
        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id);

            if (category == null) return Result<CategoryDTO>.Failure(ErrorType.NotFound);

            var categoryDTO = mapper.Map<CategoryDTO>(category);
            return Result<CategoryDTO>.Success(categoryDTO);
        }
        public async Task AddCategoryAsync(CreateCategoryRequest category)
        {
            // mapper 
            var newCategory = mapper.Map<Category>(category);
            await categoryRepository.AddCategoryAsync(newCategory);
        }
        public async Task<Result<CategoryDTO>> DeleteCategoryAsync(Guid id)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id);
            if (category == null) return Result<CategoryDTO>.Failure(ErrorType.NotFound);

            await categoryRepository.DeleteCategoryAsync(category);
            return Result<CategoryDTO>.Success(mapper.Map<CategoryDTO>(category));
        }

        public async Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request)
        {
            var existingCategory = await categoryRepository.GetCategoryByIdAsync(id);
            if (existingCategory == null) return Result<CategoryDTO>.Failure(ErrorType.NotFound);

            var categoryMapped = mapper.Map(request, existingCategory);
            await categoryRepository.UpdateCategoryAsync(categoryMapped);
            return Result<CategoryDTO>.Success(mapper.Map<CategoryDTO>(categoryMapped));
        }
    }
}
