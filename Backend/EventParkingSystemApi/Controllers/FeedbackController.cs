using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EventParkingSystemApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;

    public FeedbackController(IFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpPost]
    public async Task<ActionResult<FeedbackResponse>> Create(
        CreateFeedbackRequest request)
    {
        var customerId = GetCustomerId();

        var result = await _feedbackService.CreateAsync(
            customerId,
            request);

        return Ok(result);
    }

    [HttpGet("{feedbackId:int}")]
    public async Task<ActionResult<FeedbackResponse>> GetById(
        int feedbackId)
    {
        var result = await _feedbackService.GetByIdAsync(feedbackId);

        return Ok(result);
    }

    [HttpGet("my-feedback")]
    public async Task<ActionResult<List<FeedbackResponse>>> GetMyFeedback()
    {
        var customerId = GetCustomerId();

        var result =
            await _feedbackService.GetByCustomerIdAsync(customerId);

        return Ok(result);
    }

    [HttpPut("{feedbackId:int}")]
    public async Task<IActionResult> Update(
        int feedbackId,
        UpdateFeedbackRequest request)
    {
        var customerId = GetCustomerId();

        await _feedbackService.UpdateAsync(
            feedbackId,
            customerId,
            request);

        return NoContent();
    }

    [HttpDelete("{feedbackId:int}")]
    public async Task<IActionResult> Delete(
        int feedbackId)
    {
        var customerId = GetCustomerId();

        var isAdmin = User.IsInRole("Admin");

        await _feedbackService.DeleteAsync(
            feedbackId,
            customerId,
            isAdmin);

        return NoContent();
    }

    private int GetCustomerId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);

        if (claim == null || !int.TryParse(claim.Value, out var customerId))
            throw new UnauthorizedAccessException(
                "Customer ID not found in token.");

        return customerId;
    }
}