using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeacherSuite.Application.Common.Interfaces;
using TeacherSuite.Infrastructure.Caching;
using TeacherSuite.Infrastructure.Data;

namespace TeacherSuite.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                               ?? Environment.GetEnvironmentVariable("CONNECTION_STRINGS__DefaultConnection")
                               ?? Environment.GetEnvironmentVariable("DefaultConnection");

        Guard.Against.NullOrWhiteSpace(connectionString, message: "Connection string 'DefaultConnection' not found!");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
            }));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        var redisConnectionString = configuration.GetConnectionString("RedisCache")
                                    ?? Environment.GetEnvironmentVariable("CONNECTION_STRINGS__RedisCache");

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString;
            options.InstanceName = "TeacherSuite:";
        });

#pragma warning disable EXTEXP0018
        services.AddHybridCache(options =>
        {
            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                LocalCacheExpiration = TimeSpan.FromMinutes(2),
                Expiration = TimeSpan.FromMinutes(10)
            };
        });
#pragma warning restore EXTEXP0018

        services.AddSingleton<ICacheService, CacheService>();
    }
}