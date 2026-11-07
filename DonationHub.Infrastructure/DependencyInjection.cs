using DonationHub.Application.Authentication;
using DonationHub.Application.Features.Categories;
using DonationHub.Application.Persistence;
using DonationHub.Infrastructure.Authentication;
using DonationHub.Infrastructure.Identity;
using DonationHub.Infrastructure.Identity.Seeding;
using DonationHub.Infrastructure.Persistence;
using DonationHub.Infrastructure.Persistence.Repositories;
using DonationHub.Infrastructure.Seeding;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DonationHub.Infrastructure
{
     public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<SoftDeleteInterceptor>();
                    
            services.AddDbContext<AppDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"));

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<SoftDeleteInterceptor>());
            });
            services.AddIdentityCore<ApplicationIdentityUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = false;
            }).AddRoles<IdentityRole<Guid>>().AddEntityFrameworkStores<AppDbContext>();

            services.AddScoped(typeof(IGenericRepository<>),typeof(GenericRepository<>));
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<RolesSeeder>();
            services.AddScoped<AdminSeeder>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICategoryService, CategoryService>();
            var assembly = typeof(DependencyInjection).Assembly;

            
            return services;
        }
    }
}
