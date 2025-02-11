using Backend.Data; // Import the database context
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args); // Create a web application

// Register the database connection in the services container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Controllers (for handling API requests)
builder.Services.AddControllers();

// Enable API documentation via Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build(); // Build the application

// Enable API documentation when running in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
app.UseAuthorization(); // Enable user authentication (to be implemented)
app.MapControllers(); // Map API endpoints

app.Run(); // Run the application
