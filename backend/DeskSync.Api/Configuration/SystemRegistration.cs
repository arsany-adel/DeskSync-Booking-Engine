using DeskSync.Api.Services;
using DeskSync.Api.Services.Interfaces;
using Hangfire;
using Hangfire.PostgreSql;

namespace DeskSync.Api.Configuration;

public static class SystemRegistration
{
    public static IServiceCollection AddConfigurationSettings(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<WorkerSettingsOptions>()
            .Bind(configuration.GetSection(WorkerSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<SmtpSettingsOptions>()
            .Bind(configuration.GetSection(SmtpSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<JwtSettingsOptions>()
            .Bind(configuration.GetSection(JwtSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddOptions<FrontendSettingsOptions>()
            .Bind(configuration.GetSection(FrontendSettingsOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        return services;
    }

    public static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped<ITzdbSyncService, TzdbSyncService>();

        services.AddHangfire(config =>
            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                    options.UseNpgsqlConnection(connectionString))
        );

        services.AddHangfireServer();

        return services;
    }
}