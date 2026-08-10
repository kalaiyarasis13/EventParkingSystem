using EventParkingSystemApi.IRepositories;
using EventParkingSystemApi.IServices;
using EventParkingSystemApi.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EventParkingSystemApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly ICustomerRepository _customerRepository;

        public TokenService(IConfiguration config, ICustomerRepository customerRepository)
        {
            _config = config;
            _customerRepository = customerRepository;
        }

        public async Task<string> GenerateToken(Customer customer)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiryMinutes = double.Parse(jwtSection["ExpiryMinutes"]!);
            var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, customer.CustomerId.ToString()),
                new(ClaimTypes.Email, customer.Email),
                new(ClaimTypes.Name, customer.FullName),
                new(ClaimTypes.Role, customer.Role)
            };

            var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            customer.CurrentToken = tokenString;
            customer.TokenExpiresAt = expiresAt;
            await _customerRepository.SaveChangesAsync();

            return tokenString;
        }
    }
}
