using DeskSync.Api.Data;
using DeskSync.Api.Entities;
using DeskSync.Api.Repositories;
using DeskSync.Api.Repositories.Interfaces;
using DeskSync.Api.Services;
using DeskSync.Api.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

builder.Services.AddScoped<IRoomService, RoomService>();

builder.Services.AddControllers();

builder.Services.AddAuthentication(defaultScheme: "Bearer")
    .AddBearerToken("Bearer");

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
