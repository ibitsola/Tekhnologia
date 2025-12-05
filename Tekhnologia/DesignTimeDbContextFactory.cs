using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Tekhnologia.Data;

namespace Tekhnologia
{
    // This factory is used by EF tools at design-time to create the DbContext.
    // It prefers Npgsql so generated migrations target PostgreSQL provider types.
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Allow overriding via environment variable, default to Npgsql for production migrations
            var provider = Environment.GetEnvironmentVariable("EF_DESIGN_PROVIDER") ?? "Npgsql";

            if (provider.Equals("Npgsql", StringComparison.OrdinalIgnoreCase))
            {
                // Prefer a connection string from env vars; fall back to a harmless local Postgres-style string.
                var conn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                           ?? Environment.GetEnvironmentVariable("DATABASE_URL")
                           ?? "Host=localhost;Database=tekhnologia_migrations;Username=postgres;Password=postgres";

                // If DATABASE_URL style (postgres://), convert to Npgsql connection string
                if (conn.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase)
                    || conn.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
                {
                    var uri = new Uri(conn);
                    var userInfo = uri.UserInfo.Split(':');
                    var user = Uri.UnescapeDataString(userInfo[0]);
                    var pass = userInfo.Length > 1 ? Uri.UnescapeDataString(userInfo[1]) : string.Empty;
                    var host = uri.Host;
                    var port = uri.Port > 0 ? uri.Port : 5432;
                    var database = uri.AbsolutePath.TrimStart('/');
                    conn = $"Host={host};Port={port};Database={database};Username={user};Password={pass};";
                }

                optionsBuilder.UseNpgsql(conn);
            }
            else
            {
                var sqlite = Environment.GetEnvironmentVariable("SQLite__Connection") ?? "Data Source=tekhnologia.db";
                optionsBuilder.UseSqlite(sqlite);
            }

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
