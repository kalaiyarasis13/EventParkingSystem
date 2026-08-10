using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static EventParkingSystemApi.DTOs.NotificationDtos;

namespace EventParkingSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _service;
        public NotificationsController(INotificationService service) => _service = service;

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<List<NotificationResponse>>> GetByCustomer(int customerId) =>
            Ok(await _service.GetByCustomerAsync(customerId));

        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _service.MarkAsReadAsync(id, User.GetCustomerId());
            return NoContent();
        }
    }
}
