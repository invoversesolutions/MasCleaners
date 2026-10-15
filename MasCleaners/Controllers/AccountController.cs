using MasCleaners.DataConnection;
using MasCleaners.Models;
using MasCleaners.Services;
using MasCleaners.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.Controllers
{
    public class AccountController : Controller
    {
        // ======================================================
        // DEPENDENCIES
        // ======================================================

        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly SqlDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly CartService _cartService;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            SqlDbContext context,
            ILogger<AccountController> logger,CartService cartService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _logger = logger;
            _cartService = cartService;
        }
        // ======================================================
        // LOGIN - GET
        // ======================================================
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }
            var model = new LoginVM
            {
                ReturnUrl = returnUrl
            };

            return View(model);
        }
        // ======================================================
        // LOGIN - POST
        // ======================================================
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToRoleDashboard();
            }
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var email = model.Email.Trim();

                var user = await _userManager.FindByEmailAsync(email);

                // Use a generic message so the form does not reveal
                // whether a particular email address has an account.
                if (user == null)
                {
                    _logger.LogWarning(
                        "Login failed because the account was not found.");

                    ModelState.AddModelError(
                        string.Empty,
                        "Invalid email address or password.");

                    return View(model);
                }

                //if (!user.IsActiveAccount())
                //{
                //    ModelState.AddModelError(
                //        string.Empty,
                //        "This account is currently inactive. Please contact support.");

                //    return View(model);
                //}

                var result = await _signInManager.PasswordSignInAsync(
                    user,
                    model.Password,
                    model.RememberMe,
                    lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    if(User.IsInRole(StaticRoles.Role_Customer))
                    {
                        await _cartService.MigrateGuestCartAsync(user.Id, Request, Response);
                    }
        

                    _logger.LogInformation(
                        "User {UserId} signed in successfully.",
                        user.Id);

                    // Role-based destinations take precedence over ReturnUrl.
                    return RedirectToRoleDashboard(user);
                }

                if (result.IsLockedOut)
                {
                    _logger.LogWarning(
                        "Login attempt blocked because the account is locked.");

                    ModelState.AddModelError(
                        string.Empty,
                        "This account is temporarily locked. Please try again later.");

                    return View(model);
                }

                if (result.IsNotAllowed)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "Sign-in is not currently permitted for this account.");

                    return View(model);
                }

                _logger.LogWarning(
                    "Failed login attempt for user {UserId}.",
                    user.Id);

                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email address or password.");

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");

                ModelState.AddModelError(
                    string.Empty,
                    "We could not sign you in right now. Please try again.");

                return View(model);
            }
        }
        // ======================================================
        // ROLE-BASED DASHBOARD REDIRECT
        // ======================================================
        private IActionResult RedirectToRoleDashboard(ApplicationUser? user = null)
        {
            // When called from the GET action, resolve the current user.
            if (user == null)
            {
                return RedirectToAction("Profile", "Account");
            }
            // Check the most privileged roles first.
            if (_userManager.IsInRoleAsync(user, StaticRoles.Role_Super)
                .GetAwaiter().GetResult())
            {
                return RedirectToAction("Index", "SuperDashboard");
            }

            if (_userManager.IsInRoleAsync(user, StaticRoles.Role_Admin)
                .GetAwaiter().GetResult())
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            if (_userManager.IsInRoleAsync(user, StaticRoles.Role_Compliance)
                .GetAwaiter().GetResult())
            {
                return RedirectToAction("Index", "ComplianceDashboard");
            }

            if (_userManager.IsInRoleAsync(user, StaticRoles.Role_Customer)
                .GetAwaiter().GetResult())
            {
                return RedirectToAction("Index", "Home");
            }

            _logger.LogWarning(
                "User {UserId} signed in but has no recognized application role.",
                user.Id);

            return RedirectToAction("AccessDenied", "Account");
        }

        // ======================================================
        // REGISTER - GET
        // ======================================================

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View(new RegisterVM());
        }


        // ======================================================
        // REGISTER - POST
        // ======================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // --------------------------------------------------
            // CHECK CUSTOMER ROLE
            // --------------------------------------------------

            if (!await _context.Roles.AnyAsync(
                role => role.Name == StaticRoles.Role_Customer))
            {
                _logger.LogError(
                    "Customer role is missing during registration.");

                ModelState.AddModelError(
                    string.Empty,
                    "Registration is temporarily unavailable. Please try again later.");

                return View(model);
            }

            // --------------------------------------------------
            // PREVENT DUPLICATE EMAIL
            // --------------------------------------------------

            var email = model.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                ModelState.AddModelError(
                    nameof(model.Email),
                    "An account with this email address already exists.");

                return View(model);
            }

            await using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                // ----------------------------------------------
                // CREATE IDENTITY USER
                // ----------------------------------------------

                var user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    PhoneNumber = model.PhoneNumber.Trim(),
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(
                    user,
                    model.Password);

                if (!createResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    foreach (var error in createResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }

                // ----------------------------------------------
                // ASSIGN CUSTOMER ROLE ONLY
                // ----------------------------------------------

                var roleResult = await _userManager.AddToRoleAsync(
                    user,
                    StaticRoles.Role_Customer);

                if (!roleResult.Succeeded)
                {
                    await transaction.RollbackAsync();

                    foreach (var error in roleResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    _logger.LogError(
                        "Failed to assign Customer role to {Email}.",
                        email);

                    return View(model);
                }

                // ----------------------------------------------
                // CREATE CUSTOMER PROFILE
                // ----------------------------------------------

                var customer = new Customer
                {
                    ApplicationUserId = user.Id,
                    CustomerType = model.CustomerType,

                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    PhoneNumber = model.PhoneNumber.Trim(),

                    CompanyName = model.CustomerType == CustomerType.Company
                        ? model.CompanyName?.Trim()
                        : null,

                    VatNumber = model.CustomerType == CustomerType.Company
                        ? model.VatNumber?.Trim()
                        : null,

                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    UpdatedDate = DateTime.Now
                };

                _context.Customers.Add(customer);

                await _context.SaveChangesAsync();

                // ----------------------------------------------
                // COMMIT ACCOUNT AND PROFILE
                // ----------------------------------------------

                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Customer account registered successfully: {Email}.",
                    email);

                // ----------------------------------------------
                // SIGN IN
                // ----------------------------------------------

                // Email is not confirmed yet, so the normal
                // Identity confirmation policy can be enforced
                // when email confirmation is implemented.
                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                await _cartService.MigrateGuestCartAsync(user.Id, Request, Response);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "An error occurred while registering customer {Email}.",
                    email);

                ModelState.AddModelError(
                    string.Empty,
                    "We could not complete your registration. Please try again.");

                return View(model);
            }
        }
        // ======================================================
        // LOGOUT - POST
        // ======================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userEmail = User.Identity?.Name;

            await _signInManager.SignOutAsync();

            _logger.LogInformation(
                "User {Email} signed out successfully.",
                userEmail);

            return RedirectToAction("Index", "Home");
        }
        // ======================================================
        // CUSTOMER ACCOUNT OVERVIEW
        // ======================================================

        [HttpGet]
        [Authorize(Roles = StaticRoles.Role_Customer)]
        public IActionResult Profile()
        {
            return View();
        }
    }
}