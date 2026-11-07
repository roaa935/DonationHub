
using Microsoft.AspNetCore.Identity;

namespace DonationHub.Infrastructure.Identity.Seeding;

public class RolesSeeder
{
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;

    public RolesSeeder(RoleManager<IdentityRole<Guid>> roleManager)
    {
        _roleManager = roleManager;
    }

    public async Task SeedAsync()
    {
        foreach (var roleName in Roles.AllRoles)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);

            if (!roleExists)
            {
                var role = new IdentityRole<Guid>
                {
                    Name = roleName
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        $"Failed to create role '{roleName}': " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}
