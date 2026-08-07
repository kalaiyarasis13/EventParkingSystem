using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class HoldController : ControllerBase
{
    private readonly IHoldService _holdService;

    public HoldController(IHoldService holdService)
    {
        _holdService = holdService;
    }

    [HttpPost]
    public async Task<ActionResult<HoldResponse>> Create(
        CreateHoldRequest request)
    {
        var customerId = GetCustomerId();

        var result = await _holdService.CreateAsync(
            customerId,
            request);

        return Ok(result);
    }

    [HttpGet("{holdId:int}")]
    public async Task<ActionResult<HoldResponse>> GetById(
        int holdId)
    {
        var result = await _holdService.GetByIdAsync(holdId);

        return Ok(result);
    }

    [HttpGet("my-active")]
    public async Task<ActionResult<List<HoldResponse>>> GetMyActiveHolds()
    {
        var customerId = GetCustomerId();

        var result =
            await _holdService.GetMyActiveHoldsAsync(customerId);

        return Ok(result);
    }

    [HttpDelete("{holdId:int}")]
    public async Task<IActionResult> Release(int holdId)
    {
        var customerId = GetCustomerId();

        await _holdService.ReleaseAsync(
            holdId,
            customerId);

        return NoContent();
    }

    [HttpPost("{holdId:int}/expire")]
    public async Task<IActionResult> Expire(int holdId)
    {
        await _holdService.ExpireAsync(holdId);

        return NoContent();
    }

    private int GetCustomerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null ||
            !int.TryParse(claim.Value, out var customerId))
        {
            throw new UnauthorizedAccessException(
                "Customer ID not found in token.");
        }

        return customerId;
    }
}