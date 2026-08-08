using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystem.API.Controllers;

[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _service;
    public PaymentController(IPaymentService service) => _service = service;

    [HttpGet("api/bookings/{id:int}/payment")]
    public async Task<ActionResult<PaymentDueResponse>> GetDue(int id) => Ok(await _service.GetDueAsync(id));

    [HttpPost("api/bookings/{id:int}/payment")]
    public async Task<ActionResult<PaymentResponse>> Pay(int id) => Ok(await _service.PayAsync(id));

    [HttpGet("api/payments/customer/{customerId:int}")]
    public async Task<ActionResult<List<PaymentResponse>>> GetByCustomer(int customerId) =>
        Ok(await _service.GetByCustomerAsync(customerId));

    [HttpGet("api/payments/{id:int}/receipt")]
    public async Task<ActionResult<ReceiptResponse>> GetReceipt(int id) => Ok(await _service.GetReceiptAsync(id));
}
