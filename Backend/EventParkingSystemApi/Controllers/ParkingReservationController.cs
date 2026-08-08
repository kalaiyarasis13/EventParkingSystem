using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParkingReservationController : ControllerBase
{
    private readonly IParkingReservationService _service;

    public ParkingReservationController(
        IParkingReservationService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<ParkingReservationResponse>> Create(
        CreateParkingReservationRequest request)
    {
        var customerId = GetCustomerId();

        var result = await _service.CreateAsync(
            customerId,
            request);

        return Ok(result);
    }

    [HttpGet("{parkingReservationId:int}")]
    public async Task<ActionResult<ParkingReservationResponse>> GetById(
        int parkingReservationId)
    {
        var result = await _service.GetByIdAsync(
            parkingReservationId);

        return Ok(result);
    }

    [HttpGet("booking/{bookingId:int}")]
    public async Task<ActionResult<ParkingReservationResponse>> GetByBookingId(
        int bookingId)
    {
        var result = await _service.GetByBookingIdAsync(
            bookingId);

        if (result is null)
            return NotFound("Parking reservation not found.");

        return Ok(result);
    }

    [HttpGet("my")]
    public async Task<ActionResult<List<ParkingReservationResponse>>> GetMyReservations()
    {
        var customerId = GetCustomerId();

        var result = await _service.GetMyReservationsAsync(
            customerId);

        return Ok(result);
    }

    [HttpDelete("{parkingReservationId:int}")]
    public async Task<IActionResult> Cancel(
        int parkingReservationId)
    {
        var customerId = GetCustomerId();

        await _service.CancelAsync(
            parkingReservationId,
            customerId);

        return NoContent();
    }

    private int GetCustomerId()
    {
        var claim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (claim == null ||
            !int.TryParse(claim.Value, out var customerId))
        {
            throw new UnauthorizedAccessException(
                "Customer ID not found in token.");
        }

        return customerId;
    }
}