using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices
{
    public interface IDashboardService
    {
        Task<CustomerDashboardResponse> GetCustomerDashboardAsync(int customerId);
        Task<AdminDashboardResponse> GetAdminDashboardAsync();
    }
}
