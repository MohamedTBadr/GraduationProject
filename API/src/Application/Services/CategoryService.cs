using Application;
using Application.DTOs.CategoryDTOs;
using Application.Interfaces;
using AutoMapper;
using Domain.Contracts;
using Domain.Entities;
namespace Application.Services
{
    public class CategoryService(ICategoryRepository categoryRepository, IMapper mapper) : ICategoryService
    {
        public async Task<Result<List<CategoryDTO>>> GetAllCategoriesAsync(CancellationToken cancellationToken)
        {
            var categories = await categoryRepository.GetAllCategoriesAsync(cancellationToken);
            var categoriesDTO = mapper.Map<List<CategoryDTO>>(categories);
            return Result<List<CategoryDTO>>.Success(categoriesDTO);
        }
        public async Task<Result<CategoryDTO>> GetCategoryByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id, cancellationToken);

            if (category == null) return Result<CategoryDTO>.NotFound("Category not found");

            var categoryDTO = mapper.Map<CategoryDTO>(category);
            return Result<CategoryDTO>.Success(categoryDTO);
        }
        public async Task AddCategoryAsync(CreateCategoryRequest category, CancellationToken cancellationToken)
        {
            // mapper 
            
            var newCategory = mapper.Map<Category>(category);

            await categoryRepository.AddCategoryAsync(newCategory, cancellationToken);
        }
        public async Task<Result<CategoryDTO>> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken)
        {
            var category = await categoryRepository.GetCategoryByIdAsync(id, cancellationToken);
            if (category == null) return Result<CategoryDTO>.NotFound("Category not found");

            await categoryRepository.DeleteCategoryAsync(category, cancellationToken);
            return Result<CategoryDTO>.Success(mapper.Map<CategoryDTO>(category));
        }

        public async Task<Result<CategoryDTO>> UpdateCategoryAsync(Guid id, UpdateCategoryRequest request, CancellationToken cancellationToken)
        {
            var existingCategory = await categoryRepository.GetCategoryByIdAsync(id, cancellationToken);
            if (existingCategory == null) return Result<CategoryDTO>.NotFound("Category not found");

            var categoryMapped = mapper.Map(request, existingCategory);
            await categoryRepository.UpdateCategoryAsync(categoryMapped, cancellationToken);
            return Result<CategoryDTO>.Success(mapper.Map<CategoryDTO>(categoryMapped));
        }
    }
}
