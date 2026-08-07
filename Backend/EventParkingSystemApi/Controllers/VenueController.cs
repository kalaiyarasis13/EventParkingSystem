using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VenueController : ControllerBase
{
    private readonly IVenueService _venueService;

    public VenueController(IVenueService venueService)
    {
        _venueService = venueService;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VenueResponse>> Create(
        CreateVenueRequest request)
    {
        var result = await _venueService.CreateAsync(request);

        return Ok(result);
    }

    [HttpGet("{venueId:int}")]
    public async Task<ActionResult<VenueResponse>> GetById(
        int venueId)
    {
        var result = await _venueService.GetByIdAsync(venueId);

        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<List<VenueResponse>>> GetAll()
    {
        var result = await _venueService.GetAllAsync();

        return Ok(result);
    }

    [HttpPut("{venueId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<VenueResponse>> Update(
        int venueId,
        UpdateVenueRequest request)
    {
        var result = await _venueService.UpdateAsync(
            venueId,
            request);

        return Ok(result);
    }

    [HttpDelete("{venueId:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(
        int venueId)
    {
        await _venueService.DeleteAsync(venueId);

        return NoContent();
    }
}