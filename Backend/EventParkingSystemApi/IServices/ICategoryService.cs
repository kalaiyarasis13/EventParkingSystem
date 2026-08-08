using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices
{
    public interface ICategoryService
    {
        Task<List<CategoryResponse>> GetAllAsync();
        Task<CategoryResponse> CreateAsync(CategoryCreateRequest request);
    }
}
