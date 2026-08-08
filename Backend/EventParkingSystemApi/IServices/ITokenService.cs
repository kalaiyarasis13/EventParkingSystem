using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IServices
{
    public interface ITokenService
    {
        Task<string> GenerateToken(Customer customer);
    }
}
