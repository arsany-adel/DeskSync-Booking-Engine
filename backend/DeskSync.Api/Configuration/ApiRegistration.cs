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

public static class ApiRegistration
{
    public static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                // Converts enums to strings in Swagger and JSON responses
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddProblemDetails();

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
