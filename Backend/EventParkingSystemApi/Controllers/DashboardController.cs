using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventParkingSystemApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _service;
        public DashboardController(IDashboardService service) => _service = service;

        [HttpGet("customer/{customerId:int}")]
        public async Task<ActionResult<CustomerDashboardResponse>> Customer(int customerId) =>
            Ok(await _service.GetCustomerDashboardAsync(customerId));

        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AdminDashboardResponse>> Admin() => Ok(await _service.GetAdminDashboardAsync());
    }
}
