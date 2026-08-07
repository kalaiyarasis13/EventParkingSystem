using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ParkingSlotController : ControllerBase
{
    private readonly IParkingSlotService _parkingSlotService;

    public ParkingSlotController(
        IParkingSlotService parkingSlotService)
    {
        _parkingSlotService = parkingSlotService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ParkingSlotResponse>> Create(
        CreateParkingSlotRequest request)
    {
        var result = await _parkingSlotService.CreateAsync(request);

        return Ok(result);
    }

    [HttpGet("{parkingSlotId:int}")]
    public async Task<ActionResult<ParkingSlotResponse>> GetById(
        int parkingSlotId)
    {
        var result = await _parkingSlotService.GetByIdAsync(
            parkingSlotId);

        return Ok(result);
    }

    [HttpGet("event/{eventId:int}")]
    public async Task<ActionResult<List<ParkingSlotResponse>>> GetByEvent(
        int eventId)
    {
        var result = await _parkingSlotService.GetByEventAsync(
            eventId);

        return Ok(result);
    }

    [HttpGet("event/{eventId:int}/available")]
    public async Task<ActionResult<List<ParkingSlotResponse>>> GetAvailable(
        int eventId)
    {
        var result =
            await _parkingSlotService.GetAvailableByEventAsync(
                eventId);

        return Ok(result);
    }

    [HttpPut("{parkingSlotId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<ParkingSlotResponse>> Update(
        int parkingSlotId,
        UpdateParkingSlotRequest request)
    {
        var result = await _parkingSlotService.UpdateAsync(
            parkingSlotId,
            request);

        return Ok(result);
    }

    [HttpDelete("{parkingSlotId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int parkingSlotId)
    {
        await _parkingSlotService.DeleteAsync(parkingSlotId);

        return NoContent();
    }
}