using EventParkingSystemApi.DTOs;
using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;
using Microsoft.AspNetCore.Identity.Data;

namespace EventParkingSystemApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ITokenService _tokenService;

        public AuthService(ICustomerRepository customerRepository, ITokenService tokenService)
        {
            _customerRepository = customerRepository;
            _tokenService = tokenService;
        }

        public async Task<AuthResponse> RegisterAsync(DTOs.RegisterRequest request)
        {
            // Business Rule: Customer emails must be unique across all customers.
            if (await _customerRepository.ExistsByEmailAsync(request.Email))
                throw ApiException.Conflict("A customer with this email already exists.");

            var customer = new Customer
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                PasswordHash = PasswordHelper.Hash(request.Password),
                Role = "Customer"
            };

            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            var token = _tokenService.GenerateToken(customer);
            return new AuthResponse(token, customer.CustomerId, customer.FullName, customer.Email, customer.Role);
        }

        public async Task<AuthResponse> LoginAsync(DTOs.LoginRequest request)
        {
            var customer = await _customerRepository.GetByEmailAsync(request.Email);
            if (customer is null || !PasswordHelper.Verify(request.Password, customer.PasswordHash))
                throw ApiException.Unauthorized("Invalid email or password.");

            if (!customer.IsActive)
                throw ApiException.Forbidden("This account has been deactivated.");

            var token = _tokenService.GenerateToken(customer);
            return new AuthResponse(token, customer.CustomerId, customer.FullName, customer.Email, customer.Role);
        }
    }
}
