using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeacherSuite.Infrastructure.Data;

namespace TeacherSuite.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? Environment.GetEnvironmentVariable("CONNECTION_STRINGS__DEFAULTCONNECTION")
                               ?? Environment.GetEnvironmentVariable("DefaultConnection");

        Guard.Against.NullOrWhiteSpace(connectionString, message: "Connection string 'DefaultConnection' not found!");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));
    }
}