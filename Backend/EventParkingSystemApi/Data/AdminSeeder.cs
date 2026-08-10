using EventParkingSystemApi.Helpers;
using EventParkingSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EventParkingSystemApi.Data
{
    public static class AdminSeeder
    {
        public static async Task SeedAdminAsync(AppDbContext context)
        {
            var adminEmail = "admin@parkora.com";

            var admin = await context.Customers
                .FirstOrDefaultAsync(c => c.Email == adminEmail);

            if (admin == null)
            {
                admin = new Customer
                {
                    FullName = "Parkora Admin",
                    Email = adminEmail,
                    Phone = "0771234567",
                    PasswordHash = PasswordHelper.Hash("Admin@123"),
                    Role = "Admin",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                context.Customers.Add(admin);
            }
            else
            {
                admin.Role = "Admin";
                admin.IsActive = true;
                admin.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();
        }
    }
}