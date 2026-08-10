using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices
{
    public interface ICustomerService
    {
        Task<CustomerProfileResponse> GetByIdAsync(int id);
        Task<CustomerResponse> UpdateAsync(int id, int requestingCustomerId, CustomerUpdateRequest request);
        Task<List<CustomerResponse>> SearchAsync(string? search);
        Task DeactivateAsync(int id);
    }
}
