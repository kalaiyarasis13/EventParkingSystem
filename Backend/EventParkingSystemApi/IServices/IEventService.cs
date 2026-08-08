using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices
{
    public interface IEventService
    {
        Task<List<EventResponse>> GetAllAsync(string? name, DateOnly? date, int? venueId, int? categoryId);
        Task<EventResponse> GetByIdAsync(int id);
        Task<EventResponse> CreateAsync(EventCreateRequest request);
        Task<EventResponse> UpdateAsync(int id, EventUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
