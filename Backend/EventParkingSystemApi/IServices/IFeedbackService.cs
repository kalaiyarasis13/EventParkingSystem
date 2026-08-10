using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.IServices;

public interface IFeedbackService
{
    Task<FeedbackResponse> CreateAsync(
        int customerId,
        CreateFeedbackRequest request);

    Task<FeedbackResponse> GetByIdAsync(int feedbackId);

    Task<List<FeedbackResponse>> GetAllAsync();

    Task<List<FeedbackResponse>> GetByCustomerIdAsync(int customerId);

    Task UpdateAsync(
        int feedbackId,
        int customerId,
        UpdateFeedbackRequest request);

    Task DeleteAsync(
        int feedbackId,
        int customerId,
        bool isAdmin);
}