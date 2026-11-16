using DeskSync.Api.Configuration;
using Microsoft.AspNetCore.Diagnostics;

using Hangfire;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddAppDbContext(connectionString);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddConfigurationSettings(builder.Configuration);
builder.Services.AddBackgroundJobs(connectionString);
builder.Services.AddApiControllers();
builder.Services.AddSwaggerDocumentation();

builder.Services.AddAuthentication(defaultScheme: "Bearer")
    .AddBearerToken("Bearer");
builder.Services.AddAuthorization();

var app = builder.Build();

// Maps UnauthorizedAccessException to 401 instead of 500
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var ex = feature?.Error;

        if (ex is UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
            return;
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await context.Response.WriteAsJsonAsync(new { error = "An unexpected error occurred." });
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Don't forget Hangfire Dashboard you must secure it with authentication and authorization in production

app.UseRecurringJobs();

app.Run();