using DGC.Sample.Infrastructure.Persistence.Data;
using Microsoft.EntityFrameworkCore;

namespace DGC.Sample.Api.Extensions;

public static class DatabaseExtensions
{
    public static IServiceCollection AddSqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetRequiredConnectionString(configuration);
        var resiliency = GetResiliencySettings(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(connectionString, sqlServerOptions =>
            {
                if (resiliency.CommandTimeoutSeconds > 0)
                {
                    sqlServerOptions.CommandTimeout(resiliency.CommandTimeoutSeconds);
                }

                if (resiliency.EnableRetryOnFailure)
                {
                    sqlServerOptions.EnableRetryOnFailure(
                        resiliency.MaxRetryCount,
                        TimeSpan.FromSeconds(resiliency.MaxRetryDelaySeconds),
                        errorNumbersToAdd: null);
                }
            }));

        return services;
    }

    public static IServiceCollection AddPostgresqlServer(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetRequiredConnectionString(configuration);
        var resiliency = GetResiliencySettings(configuration);

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                if (resiliency.CommandTimeoutSeconds > 0)
                {
                    npgsqlOptions.CommandTimeout(resiliency.CommandTimeoutSeconds);
                }

                if (resiliency.EnableRetryOnFailure)
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        resiliency.MaxRetryCount,
                        TimeSpan.FromSeconds(resiliency.MaxRetryDelaySeconds),
                        errorCodesToAdd: null);
                }
            }));

        return services;
    }

    private static string GetRequiredConnectionString(IConfiguration configuration)
    {
        return configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'ConnectionStrings:DefaultConnection' is required.");
    }

    private static DatabaseResiliencySettings GetResiliencySettings(IConfiguration configuration)
    {
        return new DatabaseResiliencySettings
        {
            EnableRetryOnFailure = configuration.GetValue("Database:Resiliency:EnableRetryOnFailure", true),
            MaxRetryCount = Math.Max(0, configuration.GetValue("Database:Resiliency:MaxRetryCount", 5)),
            MaxRetryDelaySeconds = Math.Max(1, configuration.GetValue("Database:Resiliency:MaxRetryDelaySeconds", 30)),
            CommandTimeoutSeconds = Math.Max(0, configuration.GetValue("Database:Resiliency:CommandTimeoutSeconds", 30))
        };
    }

    private sealed class DatabaseResiliencySettings
    {
        public bool EnableRetryOnFailure { get; init; }

        public int MaxRetryCount { get; init; }

        public int MaxRetryDelaySeconds { get; init; }

        public int CommandTimeoutSeconds { get; init; }
    }
}