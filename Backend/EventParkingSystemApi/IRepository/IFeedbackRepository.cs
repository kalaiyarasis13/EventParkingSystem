using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.IRepository;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int feedbackId);

    Task<Feedback?> GetByBookingIdAsync(int bookingId);

    Task<List<Feedback>> GetByCustomerIdAsync(int customerId);

    Task AddAsync(Feedback feedback);

    void Update(Feedback feedback);

    void Remove(Feedback feedback);

    Task<int> SaveChangesAsync();
}