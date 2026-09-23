using DeskSync.Api.Entities;
using DeskSync.Api.Services;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace DeskSync.Api.Configuration;

public static class ApplicationRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IRoomService, RoomService>();

        services.AddSingleton<IClock>(SystemClock.Instance);
        services.AddSingleton<IDateTimeZoneProvider>(DateTimeZoneProviders.Tzdb);

        return services;
    }
}