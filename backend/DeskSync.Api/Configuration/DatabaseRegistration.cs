using DeskSync.Api.Data;
using DeskSync.Api.Repositories;
using DeskSync.Api.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DeskSync.Api.Configuration;

public static class DatabaseRegistration
{
    public static IServiceCollection AddAppDbContext(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions => npgsqlOptions.UseNodaTime());
        });

        return services;
    }

    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IRoomRepository, RoomRepository>();

        return services;
    }
}