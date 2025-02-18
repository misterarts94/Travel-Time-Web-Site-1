using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TopSpeed.Domain.Common;
using TopSpeed.Domain.Models;

namespace Top_Speed.Infrastructure.Common
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) 
        { 
        
        }

    // Your DbSets here...
        

        public DbSet<Brand> Brands { get; set; }

        public DbSet<VehicleType> VehicleType { get; set; }

        public DbSet<Post> Post { get; set; }

        public object Brand { get; internal set; }

        public async Task AddAaync<T>(T entity) where T : BaseModel
        {
            await AddAsync(entity);
            await SaveChangesAsync();
        }
    }
}
