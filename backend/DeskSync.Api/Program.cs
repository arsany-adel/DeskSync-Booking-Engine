using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using DeskSync.Api.Configuration;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.UseNodaTime(); 
    });
});

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

builder.Services.Configure<WorkerSettingsOptions>(
    builder.Configuration.GetSection(WorkerSettingsOptions.SectionName));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());// Converts enums to strings in Swagger and JSON responses
    });

builder.Services.AddAuthentication(defaultScheme: "Bearer")
    .AddBearerToken("Bearer");

builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();

builder.Services.AddScoped<ITzdbSyncService, TzdbSyncService>();

//Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(options => 
        options.UseNpgsqlConnection(builder.Configuration.GetConnectionString("DefaultConnection"))));

builder.Services.AddHangfireServer();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//Hangfire
app.UseHangfireDashboard("/hangfire");
RecurringJob.AddOrUpdate<ITzdbSyncService>(
    "tzdb-daily-sync",//ID for the  background job so if the deployment is restarted, the job will not be duplicated
    service => service.SyncReservationsAsync(CancellationToken.None),
    Cron.Daily(2));// Runs daily at 2:00 AM UTC

app.Run();
