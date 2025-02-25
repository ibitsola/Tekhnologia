using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore; // Import Entity Framework Core for database operations
using Models;

namespace Data
{
    // The ApplicationDbContext class is the "bridge" between the database and the application.
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        // The constructor receives database options (like which database provider we are using - SQLite/PostgreSQL)
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // These are our "tables" in the database.
        public DbSet<JournalEntry> JournalEntries { get; set;} // Stores users' journal entries
        public DbSet<VisionBoardItem> VisionBoardItems { get; set;} // Stores vision board items (images & text)
        public DbSet<Goal> Goals { get; set; } // Stores users career goals
        public DbSet<DigitalResource> DigitalResources { get; set; }  // New table for digital resources
        public DbSet<Purchase> Purchases { get; set; }

        
        // The OnModelCreating method allows us to configure table relationships and constraints (I’ll expand it later)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Keep the base behavior for now
        }
    }
}