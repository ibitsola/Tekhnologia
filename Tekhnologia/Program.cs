using Stripe;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Tekhnologia.Services;
using Tekhnologia.Services.ApiClients;
using Tekhnologia.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using System.IO;
using Microsoft.AspNetCore.Authentication;


var builder = WebApplication.CreateBuilder(args);

// Configure data protection to persist keys to a shared folder at the repository root
var dpKeysFolder = Path.Combine(Directory.GetParent(builder.Environment.ContentRootPath)!.FullName, "DataProtection-Keys");
Directory.CreateDirectory(dpKeysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeysFolder))
    .SetApplicationName("Tekhnologia");

// ─── 1) Load secret configuration (OpenAI key from user-secrets) ─────────────────
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
var openAiKey = configuration["OpenAI:ApiKey"];

// ─── 2) Stripe setup ─────────────────────────────────────────────────────────────
var stripeSecretKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")
                    ?? builder.Configuration["Stripe:SecretKey"];
StripeConfiguration.ApiKey = stripeSecretKey;

// ─── 3) HTTP client base address (for your GoalApiService) ─────────────────────
var environment = builder.Environment.IsDevelopment() ? "Development" : "Production";
var apiBaseUrl  = builder.Configuration[$"ApiBaseUrls:{environment}"]
                 ?? "https://localhost:7136";

// ─── 4) Register your application services ─────────────────────────────────────
// AuthStateService and BlazorAuthService are only needed for Blazor frontend
// builder.Services.AddScoped<IAuthStateService, AuthStateService>();
// builder.Services.AddScoped<IBlazorAuthService, BlazorAuthService>();

builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IDigitalResourceService, DigitalResourceService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IJournalService, JournalService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IVisionBoardService, VisionBoardService>();
builder.Services.AddScoped<IConversationService, ConversationService>();

// your typed HTTP client for the external API
builder.Services.AddHttpClient<GoalApiService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
});

// ─── 5) Entity Framework & Identity ────────────────────────────────────────────
// Database context - PostgreSQL for production, SQLite for development
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DATABASE_URL");

if (builder.Environment.IsProduction())
{
    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException("Production database connection string is not set. Set ConnectionStrings__DefaultConnection or DATABASE_URL.");
    }

    if (connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
        || connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
    {
        var uri = new Uri(connectionString);
        var userInfo = uri.UserInfo.Split(':');
        var user = Uri.UnescapeDataString(userInfo[0]);
        var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
        var host = uri.Host;
        var port = uri.Port > 0 ? uri.Port : 5432;
        var database = uri.AbsolutePath.TrimStart('/');

        var npgsql = $"Host={host};Port={port};Database={database};Username={user};Password={pass};";
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(npgsql));
    }
    else
    {
        builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString));
    }
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString ?? "Data Source=tekhnologia.db"));
}

builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// configure the Identity cookie (login path, expiration, etc.)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".Tekhnologia.Identity";
    options.Cookie.HttpOnly = true;
    // Use Lax in development so browsers accept the cookie over HTTP during local dev.
    // Use None in non-development so the cookie works across origins in production.
    options.Cookie.SameSite = builder.Environment.IsDevelopment()
        ? SameSiteMode.Lax
        : SameSiteMode.None;                 // ✅ Required for cross-origin with YARP proxy

    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
        ? CookieSecurePolicy.SameAsRequest
        : CookieSecurePolicy.Always;      // Use SameAsRequest in dev so cookies work over HTTP

    // Do not set Cookie.Domain for localhost (host-only cookies). If you need to
    // share cookies across subdomains in production, set the domain via configuration
    var cookieDomain = builder.Configuration["Cookie:Domain"];
    if (!string.IsNullOrWhiteSpace(cookieDomain) && !builder.Environment.IsDevelopment())
    {
        options.Cookie.Domain = cookieDomain;
    }

    options.LoginPath = "/signin";
    options.LogoutPath = "/api/auth/logout";
    options.AccessDeniedPath = "/unauthorized";
    options.ExpireTimeSpan = TimeSpan.FromDays(14);
    options.SlidingExpiration = true;
});


builder.Services.AddAuthorization();

// ─── 7) MVC, CORS, Swagger, Blazor Server & SignalR ────────────────────────────
builder.Services.AddControllers();
builder.Services.AddCors(opts =>
{
    opts.AddPolicy("AllowFrontend", p => 
        p.WithOrigins("http://localhost:5145", "https://localhost:5145")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials()); // Important: Allow cookies to be sent
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n" +
                      "Enter: Bearer <token>",
        Name   = "Authorization",
        In     = ParameterLocation.Header,
        Type   = SecuritySchemeType.Http,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme 
            { 
                Reference = new OpenApiReference 
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                } 
            }, 
            Array.Empty<string>() 
        }
    });
});

// ─── 8) Build and apply migrations + seed roles ────────────────────────────────
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db  = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
    
    // Ensure database connection is properly established
    await db.Database.EnsureCreatedAsync();

    var roleMgr = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    foreach (var role in new[] { "Admin", "User" })
    {
        if (!await roleMgr.RoleExistsAsync(role))
            await roleMgr.CreateAsync(new IdentityRole(role));
    }

    // Create default admin user
    var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
    var adminEmail = "admin@tekhnologia.co.uk";
    var adminUser = await userMgr.FindByEmailAsync(adminEmail);
    
    if (adminUser == null)
    {
        adminUser = new User
        {
            UserName = adminEmail,
            Email = adminEmail,
            Name = "System Administrator",
            EmailConfirmed = true
        };
        
        var result = await userMgr.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userMgr.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

// ─── 9) Middleware pipeline ───────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
}

app.UseStaticFiles();
app.UseRouting();

// CORS before auth - allow frontend to send cookies
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

// ─── 10) plain GET /signout endpoint───────────────────────────────────────────────────
app.MapGet("/signout", async (HttpContext ctx) =>
{
    await ctx.SignOutAsync(IdentityConstants.ApplicationScheme);
    ctx.Response.Redirect("/signin");
});

app.MapControllers();

app.Run();