using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Top_Speed.Infrastructure.Common;
using TopSpeed.Application.ApplicationConstants;
using TopSpeed.Domain.Models; // Assuming VehicleType is defined here

namespace Top_Speed.Infrastructure.Common
{
    public static class SeedData
    {
        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
                var scope = serviceProvider.CreateScope(); // Proper scope disposal

            var RoleManager1 = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                var role = new List<IdentityRole>  // Renamed variable to plural
                {
                        // Fixed: Added .ToUpper() for NormalizedName
                     new IdentityRole { Name = CustomRole.MasterAdmin, NormalizedName = CustomRole.MasterAdmin },
                     new IdentityRole { Name = CustomRole.Admin, NormalizedName = CustomRole.Admin },
                     new IdentityRole { Name = CustomRole.Customer, NormalizedName = CustomRole.Customer },
                };

            foreach (var role1 in role)
            {
                if (!await RoleManager1.RoleExistsAsync(role1.Name))
                {
                    await RoleManager1.CreateAsync(role1);
                }
            }
         
                
            
        }
        public static async Task SeedDataAsync(ApplicationDbContext _DbContext)
        {
            // Check if the database already has any VehicleType records
            if (!_DbContext.VehicleType.Any())
            {
                // Add seed data for VehicleType
                await _DbContext.VehicleType.AddRangeAsync(
                    new VehicleType { Name = "MotorCycle" },
                    new VehicleType { Name = "Car" },
                    new VehicleType { Name = "Suv" },
                    new VehicleType { Name = "Van" },
                    new VehicleType { Name = "Sedan" },
                    new VehicleType { Name = "Truck" }
                );

                // Save changes to the database
                await _DbContext.SaveChangesAsync();
            }
        }

       
        
    }
}