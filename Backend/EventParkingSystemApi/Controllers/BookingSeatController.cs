using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class BookingSeatController : ControllerBase
{
    private readonly IBookingSeatService _bookingSeatService;

    public BookingSeatController(
        IBookingSeatService bookingSeatService)
    {
        _bookingSeatService = bookingSeatService;
    }

    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<List<BookingSeatResponse>>> GetByBooking(
        int bookingId)
    {
        var result =
            await _bookingSeatService.GetByBookingIdAsync(bookingId);

        return Ok(result);
    }

    [HttpGet("{bookingSeatId:int}")]
    public async Task<ActionResult<BookingSeatResponse>> GetById(
        int bookingSeatId)
    {
        var result =
            await _bookingSeatService.GetByIdAsync(bookingSeatId);

        return Ok(result);
    }

    [HttpDelete("{bookingSeatId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int bookingSeatId)
    {
        await _bookingSeatService.DeleteAsync(bookingSeatId);

        return NoContent();
    }
}