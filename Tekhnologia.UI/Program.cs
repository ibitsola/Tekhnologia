using Tekhnologia.UI;
using Tekhnologia.Data;
using Tekhnologia.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tekhnologia.Services;
using Tekhnologia.Services.Interfaces;
using Microsoft.AspNetCore.DataProtection;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Load the API project's user-secrets (shared UserSecretsId) so UI can
// read the same OpenAI key during local development.
builder.Configuration.AddUserSecrets("50e5a782-e909-437d-840d-ad0f7bf60032");

// Shared data protection keys for local development so both UI and backend can
// decrypt the same Identity cookie. Keys stored at repo root in DataProtection-Keys.
var dpKeysFolder = Path.Combine(Directory.GetParent(builder.Environment.ContentRootPath)!.FullName, "DataProtection-Keys");
Directory.CreateDirectory(dpKeysFolder);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dpKeysFolder))
    .SetApplicationName("Tekhnologia");

// Load secret configuration (OpenAI key from user-secrets)
var configuration = new ConfigurationBuilder()
    .AddUserSecrets<Program>()
    .Build();
var openAiKey = configuration["OpenAI:ApiKey"];

// Database context (shared with backend)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity: Required for services that inject UserManager/SignInManager
// Backend handles actual authentication, but UI services need access to user store
builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireNonAlphanumeric = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure cookie to match backend
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".Tekhnologia.Identity";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = builder.Environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
    options.Cookie.SecurePolicy = builder.Environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

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

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// HttpClient for same-origin API calls (through YARP proxy)
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped(sp => 
{
    var client = sp.GetRequiredService<IHttpClientFactory>().CreateClient();
    // Use relative URLs so requests go through YARP proxy on same origin
    client.BaseAddress = new Uri("http://localhost:5145");
    return client;
});

// YARP reverse proxy for API forwarding to backend
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Configure HttpClient to accept self-signed certificates for development
builder.Services.ConfigureHttpClientDefaults(http =>
{
    http.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    });
});

// Auth + services (using backend services through project reference)
builder.Services.AddScoped<IAuthStateService, AuthStateService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAIService, AIService>();
builder.Services.AddScoped<IGoalService, GoalService>();
builder.Services.AddScoped<IBlazorAuthService, BlazorAuthService>();
builder.Services.AddScoped<IJournalService, JournalService>();
builder.Services.AddScoped<IVisionBoardService, VisionBoardService>();
builder.Services.AddScoped<IDigitalResourceService, DigitalResourceService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve static files (do not shadow component routes) and log requests for diagnostics
app.UseStaticFiles();

// Diagnostic middleware to log when a static file path is requested
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value ?? string.Empty;
    if (path.StartsWith("/css", StringComparison.OrdinalIgnoreCase)
        || path.StartsWith("/js", StringComparison.OrdinalIgnoreCase)
        || path.EndsWith(".html", StringComparison.OrdinalIgnoreCase)
        || path.Contains("favicon"))
    {
        Console.WriteLine($"[STATIC] Requested: {path}");
    }
    await next();
});

// Routing first
app.UseRouting();

// Auth before antiforgery (required order)
app.UseAuthentication();
app.UseAuthorization();

// Antiforgery MUST come after auth + routing and before endpoints
app.UseAntiforgery();

// Proxy /api requests to backend
app.MapReverseProxy();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Simple debug endpoint to verify server sees auth cookie
app.MapGet("/debug-auth", (HttpContext ctx) =>
{
    return Results.Json(new
    {
        IsAuthenticated = ctx.User.Identity?.IsAuthenticated ?? false,
        Name = ctx.User.Identity?.Name ?? "(none)",
        Claims = ctx.User.Claims.Select(c => new { c.Type, c.Value })
    });
});

// Diagnostic: try to render the GoalTracker component server-side (for inspection)
app.MapGet("/diag/raw/goaltracker", async (HttpContext ctx) =>
{
    try
    {
        // If the app can render components to static HTML, return a minimal marker
        return Results.Content("DIAG: server is up and can respond. Attempt to load interactive route '/goaltracker' in browser.", "text/plain");
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Log startup completion
Console.WriteLine($"[INFO] Tekhnologia.UI started successfully on {string.Join(", ", app.Urls)}");
Console.WriteLine("[INFO] Press Ctrl+C to shut down.");

app.Run();
