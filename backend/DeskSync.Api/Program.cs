using DeskSync.Api.Configuration;
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

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseHangfireDashboard("/hangfire");
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ✅ All recurring jobs mapped cleanly via extension method
app.MapRecurringJobs();

app.Run();