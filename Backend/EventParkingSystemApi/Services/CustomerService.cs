
using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;

namespace EventParkingSystemApi.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IBookingRepository _bookingRepository;

        public CustomerService(ICustomerRepository customerRepository, IBookingRepository bookingRepository)
        {
            _customerRepository = customerRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<CustomerProfileResponse> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id)
                ?? throw ApiException.NotFound("Customer not found.");

            var totalBookings = await _bookingRepository.CountActiveByCustomerAsync(id);
            var upcoming = await _bookingRepository.CountUpcomingByCustomerAsync(id);

            var dto = new CustomerResponse(customer.CustomerId, customer.FullName, customer.Email,customer.Phone, customer.Role, customer.IsActive, customer.CreatedAt);

            return new CustomerProfileResponse(dto, totalBookings, upcoming);
        }

        public async Task<CustomerResponse> UpdateAsync(int id, int requestingCustomerId, CustomerUpdateRequest request)
        {
            if (id != requestingCustomerId)
                throw ApiException.Forbidden("You can only update your own profile.");

            var customer = await _customerRepository.GetByIdAsync(id)
                ?? throw ApiException.NotFound("Customer not found.");

            customer.FullName = request.FullName;
            customer.Phone = request.Phone;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.SaveChangesAsync();

            return new CustomerResponse(customer.CustomerId, customer.FullName, customer.Email,
                customer.Phone, customer.Role, customer.IsActive, customer.CreatedAt);
        }

        public async Task<List<CustomerResponse>> SearchAsync(string? search)
        {
            var customers = await _customerRepository.SearchAsync(search);
            return customers.Select(c => new CustomerResponse(c.CustomerId, c.FullName, c.Email, c.Phone, c.Role, c.IsActive, c.CreatedAt)).ToList();
        }

        public async Task DeactivateAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id)
                ?? throw ApiException.NotFound("Customer not found.");
            customer.IsActive = false;
            customer.UpdatedAt = DateTime.UtcNow;
            await _customerRepository.SaveChangesAsync();
        }
    }
}
