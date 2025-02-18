// 1. Using Statements (Organized)
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using TopSpeed.Application.Contracts.Presistence;
using TopSpeed.Application.Service;
using Top_Speed.Infrastructure.Common;
using Top_Speed.Infrastructure.Repositories;
using Top_Speed.Infrastructure.UnitOfWork;
using TopSpeed.Application.Service.Interface;
using Serilog;

// 2. WebApplication Builder
var builder = WebApplication.CreateBuilder(args);

// 3. Add Services to the Container

// 3.1. Database Context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 3.2. Identity (with Roles)
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders()
.AddDefaultUI();
// If using Razor Pages for Identity UI
//builder.Services.AddDefaultIdentity<IdentityUser>(options =>
//{
//    options.SignIn.RequireConfirmedAccount = true; // Optional, but recommended
//})
//.AddRoles<IdentityRole>() // Enables role-based authorization
//.AddEntityFrameworkStores<ApplicationDbContext>()
//.AddDefaultTokenProviders()
//.AddDefaultUI();


    
// 3.3. Cookie Configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccrssDenied";
});

// 3.4. Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// 3.5. Authorization Policies (Important!)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminAccess", policy =>
        policy.RequireRole("MasterAdmin", "Admin")); // Correct role names!

    options.AddPolicy("ContentManagement", policy =>
        policy.RequireRole("MasterAdmin", "Admin")
              .RequireClaim("Department", "Content")); // Example custom claim
});

// 3.6. Repository Registrations
builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IEmailSender, EmailSender>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.AddScoped<IUserNameService, UserNameService>();
builder.Services.AddHttpContextAccessor();

// 3.7. Controllers and Razor Pages
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// 4. Database Helper Method (Separate Function)
static async Task UpdateDatabaseAsync(IHost host)
{
    using var scope = host.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        if (context.Database.IsSqlServer())
        {
            await context.Database.MigrateAsync();
        }
        await SeedData.SeedDataAsync(context); // Assuming SeedData is defined elsewhere
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database");
    }
}

builder.Host.UseSerilog((Context, Config) =>
{
    Config.WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day);
    if(Context.HostingEnvironment.IsProduction() == false)
    {
        Config.WriteTo.Console();
    }
});

// 5. Build the WebApplication
var app = builder.Build();

// 6. Database Initialization and Seeding (Before Middleware)
await UpdateDatabaseAsync(app);  // Migrate database and seed data

using (var scope = app.Services.CreateScope()) // Seed Roles
{
    var services = scope.ServiceProvider;
    await SeedData.SeedRoles(services);
}

// 7. Configure the HTTP Request Pipeline (Middleware)

// 7.1. Exception Handling (Development Only)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// 7.2. Static Files
app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseStaticFiles();

// 7.3. Routing
app.UseRouting();

// 7.4. Authentication (Crucial: Before Authorization)
app.UseAuthentication();

// 7.5. Authorization (Crucial)
app.UseAuthorization();

// 7.6. Session (If used)
app.UseSession();

// 7.7. Endpoints (Razor Pages and Controllers)
app.MapRazorPages(); // If you're using Razor Pages

app.MapControllerRoute(
    name: "default",
    pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

// 8. Run the Application
app.Run();