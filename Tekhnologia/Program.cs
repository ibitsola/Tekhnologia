using Stripe;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Tekhnologia.Services;

var builder = WebApplication.CreateBuilder(args); // Create a web application

var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();

var apiKey = configuration["OpenAI:ApiKey"];


// Configure Stripe API Key
var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
var stripePublishableKey = Environment.GetEnvironmentVariable("STRIPE_PUBLISHABLE_KEY");

// Fallback to appsettings if environment variable is not found
if (string.IsNullOrEmpty(stripeSecretKey))
{
    stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
}
if (string.IsNullOrEmpty(stripePublishableKey))
{
    stripePublishableKey = builder.Configuration["Stripe:PublishableKey"];
}

StripeConfiguration.ApiKey = stripeSecretKey; // Set the Stripe secret key

// Register JournalService as a scoped service for journal-related business logic.
builder.Services.AddScoped<JournalService>();
// Register AuthService as a scoped service so that a new instance is created per HTTP request.
builder.Services.AddScoped<AuthService>();
// Register AuthService as a scoped service so that a new instance is created per HTTP request.
builder.Services.AddScoped<AuthService>();
// Register UserService as a scoped service for user-related business logic.
builder.Services.AddScoped<UserService>();
// Register AdminService as a scoped service for admin-related business logic.
builder.Services.AddScoped<AdminService>();
// Register VisionBoardService as a scoped service for vision board–related business logic.
builder.Services.AddScoped<VisionBoardService>();
// Register DigitalResourceService as a scoped service for digital resource business logic.
builder.Services.AddScoped<DigitalResourceService>();
// Register GoalService as a scoped service for goal-related business logic.
builder.Services.AddScoped<GoalService>();
// Register PaymentService as a scoped service for payment-related business logic.
builder.Services.AddScoped<PaymentService>();


// Register the database connection in the services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
// Configure Identity for user authentication and role management
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Read JWT settings from `appsettings.json`
var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Secret"] ?? "your-very-long-secret-key-1234567890!@#ABCDEF");

// Configure Authentication & JWT Bearer Token setup
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Disable HTTPS enforcement
    options.SaveToken = true; // Saves the token in the authentication properties
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key), // Secure JWT key from config 
        ValidateIssuer = true, // Validate the issuer (API)
        ValidateAudience = true, // Validate the audience (clients)
        ValidIssuer = jwtSettings["Issuer"], // Read from `appsettings.json` 
        ValidAudience = jwtSettings["Audience"], // Read from `appsettings.json`
        ValidateLifetime = true, // Ensure the token is not expired
        ClockSkew = TimeSpan.Zero // Prevents expired tokens from being accepted due to time differences
    };    
});

builder.Services.AddAuthorization(); // Enables authorization policies

// Add CORS policy to allow frontend requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


// Register Controllers (for handling API requests)
builder.Services.AddControllers();


// Enable API documentation via Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<AIService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your token.\nExample: Bearer YOUR_TOKEN_HERE"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});


var app = builder.Build(); // Build the application
// Apply pending migrations automatically
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Roles are created when the app starts
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

    string[] roleNames = { "Admin", "User" };

    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

// This code creates an Admin user in the database if no admin exists.
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    // Instead of checking by email, check if there are any users in the "Admin" role.
    var admins = await userManager.GetUsersInRoleAsync("Admin");
    
    if (!admins.Any())
    {
        string adminEmail = "admin@example.com";
        string adminPassword = "Admin@1234";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new User
            {
                UserName = adminEmail,
                Email = adminEmail,
                Name = "Admin User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                // Assign the "Admin" role to the newly created admin user.
                await userManager.AddToRoleAsync(admin, "Admin");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    Console.WriteLine(error.Description);
                }
            }
        }
    }
}


// Enable API documentation when running in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (!app.Environment.IsDevelopment()) // Enforce HTTPS only in production
{
    app.UseHttpsRedirection();  // Redirect HTTP to HTTPS
}
app.UseCors("AllowAllOrigins"); // Enable CORS before authentication
app.UseAuthentication(); // Ensure Authentication
app.UseAuthorization(); // Enable Authorization Middleware
app.MapControllers(); // Map API endpoints

app.Run(); // Run the application