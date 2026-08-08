using EventParkingSystemApi.DTOs;

namespace EventParkingSystemApi.Services;

public interface IPaymentService
{
    Task<PaymentDueResponse> GetDueAsync(int bookingId);
    Task<PaymentResponse> PayAsync(int bookingId);
    Task<List<PaymentResponse>> GetByCustomerAsync(int customerId);
    Task<ReceiptResponse> GetReceiptAsync(int paymentId);
}
