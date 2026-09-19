using System.Text.Json.Serialization;
using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services;
using DeskSync.Api.Services.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace DeskSync.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IReservationService, ReservationService>();

        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IDateTimeZoneProvider>(DateTimeZoneProviders.Tzdb);

        return services;
    }

    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNodaTime());
        });

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();

        services.Configure<WorkerSettingsOptions>(
            configuration.GetSection(WorkerSettingsOptions.SectionName)
        );

        services.AddScoped<ITzdbSyncService, TzdbSyncService>();

        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => options.UseNpgsqlConnection(connectionString))
        );

        services.AddHangfireServer();

        return services;
    }

    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());// Converts enums to strings in Swagger and JSON responses
            });

        services.AddProblemDetails();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
