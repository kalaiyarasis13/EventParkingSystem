using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepositories;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int feedbackId);

    Task<Feedback?> GetByBookingIdAsync(int bookingId);

    Task<List<Feedback>> GetByCustomerIdAsync(int customerId);

    Task<List<Feedback>> GetAllAsync();

    Task AddAsync(Feedback feedback);

    void Update(Feedback feedback);

    void Remove(Feedback feedback);

    Task<int> SaveChangesAsync();
}