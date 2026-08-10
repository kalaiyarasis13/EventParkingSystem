using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryService(ICategoryRepository categoryRepository) => _categoryRepository = categoryRepository;

        public async Task<List<CategoryResponse>> GetAllAsync()
        {
            var categories = await _categoryRepository.GetAllOrderedAsync();
            return categories.Select(c => new CategoryResponse(c.CategoryId, c.Name)).ToList();
        }

        public async Task<CategoryResponse> CreateAsync(CategoryCreateRequest request)
        {
            if (await _categoryRepository.ExistsByNameAsync(request.Name))
                throw ApiException.Conflict("This category already exists.");

            var category = new EventCategory { Name = request.Name };
            await _categoryRepository.AddAsync(category);
            await _categoryRepository.SaveChangesAsync();
            return new CategoryResponse(category.CategoryId, category.Name);
        }
    }
}
