using DeskSync.Api.Configuration;
using DeskSync.Api.Services.Interfaces;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddPresentationServices();
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddAuthentication(defaultScheme: "Bearer")
    .AddBearerToken("Bearer"); 
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler(); // Required for your ErrorOr mapping

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Don't forget Hangfire Dashboard you must secure it with authentication and authorization in production


RecurringJob.AddOrUpdate<ITzdbSyncService>(
    "tzdb-daily-sync",
    service => service.SyncReservationsAsync(CancellationToken.None),
    Cron.Daily(2));

app.Run();