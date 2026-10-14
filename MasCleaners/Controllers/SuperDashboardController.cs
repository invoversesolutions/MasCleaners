
using MasCleaners.Models;
using MasCleaners.ViewModels.SuperUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MasCleaners.Controllers
{
    [Authorize(Roles = StaticRoles.Role_Super)]
    public class SuperDashboardController : Controller
    {
        private readonly ILogger<SuperDashboardController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly RoleManager<IdentityRole> _roleManager;


        public SuperDashboardController(
            ILogger<SuperDashboardController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            _logger.LogInformation(
                "Loading Super User dashboard for {User}",
                User.Identity?.Name);

            return View();
        }
        [HttpGet]
        public async Task<IActionResult> Users(string? searchTerm)
        {
            try
            {
                _logger.LogInformation(
                    "Loading user management for {User}",
                    User.Identity?.Name);

                var query = _userManager.Users.AsNoTracking();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var term = searchTerm.Trim();

                    query = query.Where(user =>
                        (user.Email != null && user.Email.Contains(term)) ||
                        (user.UserName != null && user.UserName.Contains(term)) ||
                        (user.PhoneNumber != null &&
                         user.PhoneNumber.Contains(term)));
                }

                var identityUsers = await query
                    .OrderBy(user => user.Email)
                    .ToListAsync();

                var model = new UserManagementVM
                {
                    SearchTerm = searchTerm?.Trim() ?? string.Empty,
                    TotalUsers = identityUsers.Count,
                    ConfirmedUsers = identityUsers.Count(
                        user => user.EmailConfirmed),
                    UnconfirmedUsers = identityUsers.Count(
                        user => !user.EmailConfirmed)
                };

                foreach (var user in identityUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);

                    model.Users.Add(new ManagedUserVM
                    {
                        Id = user.Id,
                        FullName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        EmailConfirmed = user.EmailConfirmed,
                        IsLockedOut = await _userManager.IsLockedOutAsync(user),
                        IsCurrentUser = user.Id == _userManager.GetUserId(User),
                        Roles = roles.ToList()
                    });
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load user management for {User}",
                    User.Identity?.Name);

                TempData["ErrorMessage"] =
                    "Unable to load users. Please try again.";

                return View(new UserManagementVM());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return NotFound();

                // Prevent locking the currently signed-in account.
                if (_userManager.GetUserId(User) == user.Id)
                {
                    TempData["ErrorMessage"] =
                        "You cannot lock your own account while signed in.";

                    return RedirectToAction(nameof(Users));
                }

                var roles = await _userManager.GetRolesAsync(user);

                // Protect the last Super User account from being locked.
                if (roles.Contains(StaticRoles.Role_Super))
                {
                    var superUsers = await _userManager.GetUsersInRoleAsync(
                        StaticRoles.Role_Super);

                    var availableSuperUsers = 0;

                    foreach (var superUser in superUsers)
                    {
                        if (await _userManager.IsLockedOutAsync(superUser))
                            continue;

                        availableSuperUsers++;
                    }

                    if (availableSuperUsers <= 1)
                    {
                        TempData["ErrorMessage"] =
                            "The last available Super User account cannot be locked.";

                        return RedirectToAction(nameof(Users));
                    }
                }

                if (!user.LockoutEnabled)
                {
                    var enabledResult = await _userManager.SetLockoutEnabledAsync(
                        user, true);

                    if (!enabledResult.Succeeded)
                    {
                        TempData["ErrorMessage"] =
                            "Unable to enable account lockout.";

                        return RedirectToAction(nameof(Users));
                    }
                }

                var result = await _userManager.SetLockoutEndDateAsync(
                    user, DateTimeOffset.MaxValue);

                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        string.Join(" ", result.Errors.Select(e => e.Description));

                    return RedirectToAction(nameof(Users));
                }

                _logger.LogWarning(
                    "Super User {Actor} locked account {TargetUserId}.",
                    User.Identity?.Name,
                    user.Id);

                TempData["SuccessMessage"] =
                    "The user account has been locked.";

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to lock user {UserId}.", id);

                TempData["ErrorMessage"] =
                    "Unable to lock the user account.";

                return RedirectToAction(nameof(Users));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlockUser(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
                return BadRequest();

            try
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                    return NotFound();

                var result = await _userManager.SetLockoutEndDateAsync(
                    user, null);

                if (!result.Succeeded)
                {
                    TempData["ErrorMessage"] =
                        string.Join(" ", result.Errors.Select(e => e.Description));

                    return RedirectToAction(nameof(Users));
                }

                await _userManager.ResetAccessFailedCountAsync(user);

                _logger.LogWarning(
                    "Super User {Actor} unlocked account {TargetUserId}.",
                    User.Identity?.Name,
                    user.Id);

                TempData["SuccessMessage"] =
                    "The user account has been unlocked.";

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to unlock user {UserId}.", id);

                TempData["ErrorMessage"] =
                    "Unable to unlock the user account.";

                return RedirectToAction(nameof(Users));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Roles()
        {
            try
            {
                _logger.LogInformation(
                    "Loading role management for {User}",
                    User.Identity?.Name);

                var identityRoles = await _roleManager.Roles
                    .AsNoTracking()
                    .OrderBy(role => role.Name)
                    .ToListAsync();

                var model = new RoleManagementVM
                {
                    TotalRoles = identityRoles.Count
                };

                foreach (var role in identityRoles)
                {
                    var roleName = role.Name ?? string.Empty;
                    var assignedUsers =
                        await _userManager.GetUsersInRoleAsync(roleName);

                    model.Roles.Add(new ManagedRoleVM
                    {
                        Id = role.Id,
                        Name = roleName,
                        UserCount = assignedUsers.Count,
                        IsSystemRole = IsSystemRole(roleName)
                    });
                }

                model.RolesWithoutUsers = model.Roles.Count(
                    role => role.UserCount == 0);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to load role management for {User}",
                    User.Identity?.Name);

                TempData["ErrorMessage"] =
                    "Unable to load system roles. Please try again.";

                return View(new RoleManagementVM());
            }
        }

        private static bool IsSystemRole(string roleName)
        {
            return roleName == StaticRoles.Role_Super
                || roleName == StaticRoles.Role_Admin;
        }

    }
}
