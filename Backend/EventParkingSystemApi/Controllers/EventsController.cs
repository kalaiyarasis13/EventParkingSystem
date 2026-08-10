using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _service;
        public EventsController(IEventService service) => _service = service;

        [HttpGet]
        public async Task<ActionResult<List<EventResponse>>> GetAll(
            [FromQuery] string? name, [FromQuery] DateOnly? date, [FromQuery] int? venueId, [FromQuery] int? categoryId) =>
            Ok(await _service.GetAllAsync(name, date, venueId, categoryId));

        [HttpGet("{id:int}")]
        public async Task<ActionResult<EventResponse>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EventResponse>> Create(EventCreateRequest request)
        {
            var result = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.EventId }, result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<EventResponse>> Update(int id, EventUpdateRequest request) =>
            Ok(await _service.UpdateAsync(id, request));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }
    }
}
