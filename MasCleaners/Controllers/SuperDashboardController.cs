
using MasCleaners.Interfaces;
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
        private readonly IUnitOfWork _unitOfWork;


        public SuperDashboardController(
            ILogger<SuperDashboardController> logger,
            UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
            _unitOfWork = unitOfWork;
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
        private async Task<List<string>> GetEmployeeRolesAsync()
        {
            return await _roleManager.Roles
                .Where(role =>
                    role.Name != null &&
                    role.Name != "Customer" &&
                    role.Name != "Super")
                .OrderBy(role => role.Name)
                .Select(role => role.Name!)
                .ToListAsync();
        }

        [HttpGet]
        public async Task<IActionResult> CreateEmployee()
        {
            var model = new CreateEmployeeVM
            {
                AvailableRoles = await GetEmployeeRolesAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeVM model)
        {
            model.AvailableRoles = await GetEmployeeRolesAsync();

            if (!ModelState.IsValid)
                return View(model);

            // Validate the selected employee role against the allowed roles.
            var selectedRole = await _roleManager.FindByNameAsync(
                model.SelectedRole);

            if (selectedRole?.Name == null ||
                !model.AvailableRoles.Contains(
                    selectedRole.Name,
                    StringComparer.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(
                    nameof(model.SelectedRole),
                    "Please select a valid employee role.");

                return View(model);
            }

            try
            {
                var email = model.Email.Trim();

                // Create the Identity user.
                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = model.PhoneNumber.Trim(),
                    LockoutEnabled = true
                };

                var createResult = await _userManager.CreateAsync(
                    user,
                    model.Password);

                if (!createResult.Succeeded)
                {
                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                // Assign the selected role.
                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    selectedRole.Name);

                if (!roleResult.Succeeded)
                {
                    // Compensate for the already-created Identity account.
                    var deleteResult = await _userManager.DeleteAsync(user);

                    if (!deleteResult.Succeeded)
                    {
                        _logger.LogError(
                            "Could not remove user {UserId} after role assignment failed.",
                            user.Id);
                    }

                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                // Create the employee profile linked to the Identity user.
                var employee = new Employee
                {
                    ApplicationUserId = user.Id,
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),

                    EmployeeNumber = string.IsNullOrWhiteSpace(model.EmployeeNumber)
    ? await GenerateEmployeeNumberAsync()
    : model.EmployeeNumber.Trim(),

                    JobTitle = string.IsNullOrWhiteSpace(model.JobTitle)
                        ? null
                        : model.JobTitle.Trim(),

                    Department = string.IsNullOrWhiteSpace(model.Department)
                        ? null
                        : model.Department.Trim(),

                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                await _unitOfWork.Employee.AddAsync(employee);
                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Employee {EmployeeId} and user {UserId} created with role {Role} by {Actor}.",
                    employee.Id,
                    user.Id,
                    selectedRole.Name,
                    User.Identity?.Name);

                TempData["SuccessMessage"] =
                    "Employee account created successfully.";

                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to create an employee account for {Email}.",
                    model.Email);

                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred while creating the employee account. Please check the user list before trying again.");

                return View(model);
            }
        }
        private async Task<string> GenerateEmployeeNumberAsync()
        {
            string employeeNumber;

            do
            {
                employeeNumber = $"MAS{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

            } while (await _unitOfWork.Employee
                .AnyAsync(e => e.EmployeeNumber == employeeNumber));

            return employeeNumber;
        }
    }
}
