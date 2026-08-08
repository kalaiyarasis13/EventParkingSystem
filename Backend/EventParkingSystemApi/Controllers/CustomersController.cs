using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomerService _service;
        public CustomersController(ICustomerService service) => _service = service;

        // POST /api/customers/register is handled by AuthController; kept here per BRD naming for documentation clarity.

        [HttpGet("{id:int}")]
        public async Task<ActionResult<CustomerProfileResponse>> GetById(int id) =>
            Ok(await _service.GetByIdAsync(id));

        [HttpPut("{id:int}")]
        public async Task<ActionResult<CustomerResponse>> Update(int id, CustomerUpdateRequest request) =>
            Ok(await _service.UpdateAsync(id, User.GetCustomerId(), request));

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<List<CustomerResponse>>> Search([FromQuery] string? search) =>
            Ok(await _service.SearchAsync(search));

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Deactivate(int id)
        {
            await _service.DeactivateAsync(id);
            return NoContent();
        }
    }
}
