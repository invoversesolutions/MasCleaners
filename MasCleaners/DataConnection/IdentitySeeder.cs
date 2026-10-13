using MasCleaners.Models;
using Microsoft.AspNetCore.Identity;

namespace MasCleaners.DataConnection
{
    public static class IdentitySeeder
    {
        // ======================================================
        // SEED IDENTITY ROLES
        // ======================================================

        public static async Task SeedRolesAsync(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var roleName in StaticRoles.AllRoles)
            {
                if (await roleManager.RoleExistsAsync(roleName))
                {
                    continue;
                }

                var result = await roleManager.CreateAsync(
                    new IdentityRole(roleName));

                if (result.Succeeded)
                {
                    logger.LogInformation(
                        "Created Identity role {RoleName}.",
                        roleName);
                }
                else
                {
                    var errors = string.Join(
                        "; ",
                        result.Errors.Select(error => error.Description));

                    logger.LogError(
                        "Failed to create Identity role {RoleName}: {Errors}",
                        roleName,
                        errors);

                    throw new InvalidOperationException(
                        $"Unable to create Identity role '{roleName}': {errors}");
                }
            }
        }


        // ======================================================
        // SEED SUPER USER
        // ======================================================

        public static async Task SeedSuperUserAsync(
            IServiceProvider serviceProvider,
            ILogger logger)
        {
            using var scope = serviceProvider.CreateScope();

            var configuration = scope.ServiceProvider
                .GetRequiredService<IConfiguration>();

            var userManager = scope.ServiceProvider
                .GetRequiredService<UserManager<ApplicationUser>>();

            // --------------------------------------------------
            // READ SECURE CONFIGURATION
            // --------------------------------------------------

            var email = configuration["Identity:SuperUser:Email"];
            var password = configuration["Identity:SuperUser:Password"];

            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "Super user credentials are missing. Configure " +
                    "'Identity:SuperUser:Email' and " +
                    "'Identity:SuperUser:Password' using a secure " +
                    "configuration provider.");
            }

            // --------------------------------------------------
            // ENSURE SUPER ROLE EXISTS
            // --------------------------------------------------

            var roleManager = scope.ServiceProvider
                .GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync(StaticRoles.Role_Super))
            {
                throw new InvalidOperationException(
                    "The Super role does not exist. Seed Identity roles first.");
            }

            // --------------------------------------------------
            // FIND EXISTING SUPER USER
            // --------------------------------------------------

            var user = await userManager.FindByEmailAsync(email);

            // --------------------------------------------------
            // CREATE SUPER USER IF MISSING
            // --------------------------------------------------

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var createResult = await userManager.CreateAsync(
                    user,
                    password);

                if (!createResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        createResult.Errors.Select(error => error.Description));

                    logger.LogError(
                        "Failed to create Super user {Email}: {Errors}",
                        email,
                        errors);

                    throw new InvalidOperationException(
                        $"Unable to create Super user: {errors}");
                }

                logger.LogInformation(
                    "Created initial Super user {Email}.",
                    email);
            }

            // --------------------------------------------------
            // ASSIGN SUPER ROLE
            // --------------------------------------------------

            if (!await userManager.IsInRoleAsync(
                user,
                StaticRoles.Role_Super))
            {
                var roleResult = await userManager.AddToRoleAsync(
                    user,
                    StaticRoles.Role_Super);

                if (!roleResult.Succeeded)
                {
                    var errors = string.Join(
                        "; ",
                        roleResult.Errors.Select(error => error.Description));

                    logger.LogError(
                        "Failed to assign Super role to {Email}: {Errors}",
                        email,
                        errors);

                    throw new InvalidOperationException(
                        $"Unable to assign Super role: {errors}");
                }

                logger.LogInformation(
                    "Assigned Super role to {Email}.",
                    email);
            }
        }
    }
}