using DonationHub.Api;
using DonationHub.Application.Options;
using DonationHub.Application.Persistence;
using DonationHub.Domain.Entities;
using DonationHub.Infrastructure;
using DonationHub.Infrastructure.Identity.Seeding;
using DonationHub.Infrastructure.Seeding;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructureServices(builder.Configuration);


builder.Services.AddOptions<JwtOptions>()
    .BindConfiguration("Jwt")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var jwtSettings = builder.Configuration
    .GetSection("Jwt")
    .Get<JwtOptions>()!;

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            ValidateLifetime = true,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),

            ClockSkew = TimeSpan.Zero,

            NameClaimType = "UserId",
            RoleClaimType = ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOptions<SeedAdminOptions>()
    .BindConfiguration("SeedAdmin")
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<DonationOptions>()
    .BindConfiguration("Donation")
    .ValidateDataAnnotations()
    .ValidateOnStart();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
//seed roles on application startup
using (var scope = app.Services.CreateScope())
{
    var rolesSeeder = scope.ServiceProvider
        .GetRequiredService<RolesSeeder>();

    await rolesSeeder.SeedAsync();
    var adminSeeder = scope.ServiceProvider
        .GetRequiredService<AdminSeeder>();

    await adminSeeder.SeedAsync();
}


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


