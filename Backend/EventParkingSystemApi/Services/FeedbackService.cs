using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;

namespace EventParkingSystemApi.Services;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IBookingRepository _bookingRepository;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IBookingRepository bookingRepository)
    {
        _feedbackRepository = feedbackRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<FeedbackResponse> CreateAsync(
        int customerId,
        CreateFeedbackRequest request)
    {
        // Check booking
        var booking = await _bookingRepository.GetByIdWithFullDetailsAsync(request.BookingId)
            ?? throw ApiException.NotFound("Booking not found.");

        // Customer can only give feedback for their own booking
        if (booking.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only give feedback for your own booking.");

        // Payment must be completed
        if (booking.Payment is null ||
            booking.Payment.Status != PaymentStatus.Completed)
        {
            throw ApiException.Conflict(
                "Feedback can only be submitted after payment is completed.");
        }

        // Booking must be confirmed
        if (booking.Status != BookingStatus.Confirmed)
        {
            throw ApiException.Conflict(
                "Feedback can only be submitted for a confirmed booking.");
        }

        // Only one feedback per booking
        var existingFeedback =
            await _feedbackRepository.GetByBookingIdAsync(request.BookingId);

        if (existingFeedback is not null)
            throw ApiException.Conflict(
                "Feedback has already been submitted for this booking.");

        // Rating validation
        if (request.Rating < 1 || request.Rating > 5)
            throw ApiException.BadRequest(
                "Rating must be between 1 and 5.");

        var feedback = new Feedback
        {
            BookingId = request.BookingId,
            CustomerId = customerId,
            Rating = request.Rating,
            Comment = request.Comment,
            CreatedAt = DateTime.UtcNow
        };

        await _feedbackRepository.AddAsync(feedback);
        await _feedbackRepository.SaveChangesAsync();

        var savedFeedback =
            await _feedbackRepository.GetByIdAsync(feedback.FeedbackId);

        return MapToResponse(savedFeedback!);
    }

    public async Task<FeedbackResponse> GetByIdAsync(int feedbackId)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId)
            ?? throw ApiException.NotFound("Feedback not found.");

        return MapToResponse(feedback);
    }

    public async Task<List<FeedbackResponse>> GetByCustomerIdAsync(
        int customerId)
    {
        var feedbacks =
            await _feedbackRepository.GetByCustomerIdAsync(customerId);

        return feedbacks
            .Select(MapToResponse)
            .ToList();
    }

    public async Task UpdateAsync(
        int feedbackId,
        int customerId,
        UpdateFeedbackRequest request)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId)
            ?? throw ApiException.NotFound("Feedback not found.");

        // Customer can update only their own feedback
        if (feedback.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only update your own feedback.");

        // Rating validation
        if (request.Rating < 1 || request.Rating > 5)
            throw ApiException.BadRequest(
                "Rating must be between 1 and 5.");

        feedback.Rating = request.Rating;
        feedback.Comment = request.Comment;

        _feedbackRepository.Update(feedback);

        await _feedbackRepository.SaveChangesAsync();
    }

    public async Task DeleteAsync(
        int feedbackId,
        int customerId,
        bool isAdmin)
    {
        var feedback = await _feedbackRepository.GetByIdAsync(feedbackId)
            ?? throw ApiException.NotFound("Feedback not found.");

        // Customer can delete only their own feedback
        if (!isAdmin && feedback.CustomerId != customerId)
            throw ApiException.Forbidden(
                "You can only delete your own feedback.");

        _feedbackRepository.Remove(feedback);

        await _feedbackRepository.SaveChangesAsync();
    }

    public async Task<List<FeedbackResponse>> GetAllAsync()
    {
        var feedbacks = await _feedbackRepository.GetAllAsync();

        return feedbacks
            .Select(MapToResponse)
            .ToList();
    }

    private static FeedbackResponse MapToResponse(Feedback feedback)
    {
        return new FeedbackResponse(
            feedback.FeedbackId,
            feedback.BookingId,
            feedback.CustomerId,
            feedback.Customer?.FullName ?? "Customer",
            feedback.Booking?.Event?.Name ?? "Event",
            feedback.Rating,
            feedback.Comment,
            feedback.CreatedAt
        );
    }
}