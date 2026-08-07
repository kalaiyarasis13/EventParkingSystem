using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SeatController : ControllerBase
{
    private readonly ISeatService _seatService;

    public SeatController(ISeatService seatService)
    {
        _seatService = seatService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeatResponse>> Create(
        CreateSeatRequest request)
    {
        var result = await _seatService.CreateAsync(request);

        return Ok(result);
    }

    [HttpGet("{seatId:int}")]
    public async Task<ActionResult<SeatResponse>> GetById(
        int seatId)
    {
        var result = await _seatService.GetByIdAsync(seatId);

        return Ok(result);
    }

    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<List<SeatResponse>>> GetByEvent(
        int eventId)
    {
        var result = await _seatService.GetByEventAsync(eventId);

        return Ok(result);
    }

    [HttpGet("event/{eventId:int}/available")]
    public async Task<ActionResult<List<SeatResponse>>> GetAvailable(
        int eventId)
    {
        var result =
            await _seatService.GetAvailableByEventAsync(eventId);

        return Ok(result);
    }

    [HttpPut("{seatId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SeatResponse>> Update(
        int seatId,
        UpdateSeatRequest request)
    {
        var result = await _seatService.UpdateAsync(
            seatId,
            request);

        return Ok(result);
    }

    [HttpDelete("{seatId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int seatId)
    {
        await _seatService.DeleteAsync(seatId);

        return NoContent();
    }
}