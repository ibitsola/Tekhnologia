using Stripe;
using Data;
using Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args); // Create a web application

// Configure Stripe API Key
//var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
    //StripeConfiguration.ApiKey = stripeSecretKey;

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