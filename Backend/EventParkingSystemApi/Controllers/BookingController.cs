using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.Services;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _service;
    public BookingsController(IBookingService service) => _service = service;

    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(CreateBookingRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.BookingId }, result);
    }

    [HttpGet("customer/{customerId:int}")]
    public async Task<ActionResult<List<BookingResponse>>> GetByCustomer(int customerId) =>
        Ok(await _service.GetByCustomerAsync(customerId));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BookingResponse>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Cancel(int id)
    {
        await _service.CancelAsync(id, User.GetCustomerId(), User.IsAdmin());
        return NoContent();
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<List<BookingResponse>>> GetByEvent([FromQuery] int eventId) =>
        Ok(await _service.GetByEventAsync(eventId));
}
