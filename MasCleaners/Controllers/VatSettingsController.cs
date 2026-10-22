
using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels.SuperUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MasCleaners.Controllers
{
    [Authorize(Roles = StaticRoles.Role_Super)]
    public class VatSettingsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VatSettingsController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;

        public VatSettingsController(
            IUnitOfWork unitOfWork,
            ILogger<VatSettingsController> logger,
            UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation(
                    "Loading VAT settings.");

                var settings = await _unitOfWork.VatSetting
                    .GetAllAsync();

                var current = settings
                    .OrderByDescending(x => x.VatEffectiveDate)
                    .ThenByDescending(x => x.Id)
                    .FirstOrDefault();

                var model = current == null
                    ? new VatSettingsVM()
                    : new VatSettingsVM
                    {
                        Id = current.Id,
                        IsVatRegistered = current.IsVatRegistered,
                        VatRegistrationNumber =
                            current.VatRegistrationNumber,
                        VatRate = current.VatRate,
                        VatEffectiveDate = (DateTime)current.VatEffectiveDate,
                        Notes = current.Notes
                    };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Failed to load VAT settings.");

                TempData["ErrorMessage"] =
                    "Unable to load VAT settings.";

                return View(new VatSettingsVM());
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(VatSettingsVM model)
        {
            if (model.IsVatRegistered &&
                string.IsNullOrWhiteSpace(
                    model.VatRegistrationNumber))
            {
                ModelState.AddModelError(
                    nameof(model.VatRegistrationNumber),
                    "Enter the VAT registration number.");
            }

            if (model.VatEffectiveDate == default)
            {
                ModelState.AddModelError(
                    nameof(model.VatEffectiveDate),
                    "Select a valid effective date.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var settings = await _unitOfWork.VatSetting
                    .GetAllAsync();

                var existing = settings.FirstOrDefault(
                    x => x.Id == model.Id);

                // Never create duplicate initial settings when
                // another record already exists.
                if (model.Id == 0 && existing == null)
                {
                    existing = settings
                        .OrderByDescending(x => x.Id)
                        .FirstOrDefault();
                }

                var isNew = existing == null;

                if (isNew)
                {
                    existing = new VatSetting
                    {
                        CreatedDate = DateTime.UtcNow
                    };
                }

                existing.IsVatRegistered =
                    model.IsVatRegistered;

                existing.VatRegistrationNumber =
                    model.IsVatRegistered
                        ? model.VatRegistrationNumber?.Trim()
                        : null;

                existing.VatRate = model.VatRate;
                existing.VatEffectiveDate =
                    model.VatEffectiveDate.Date;
                existing.Notes = model.Notes?.Trim();
                existing.UpdatedDate = DateTime.UtcNow;
                existing.UpdatedByUserId =
                    _userManager.GetUserId(User);

                if (isNew)
                {
                   await _unitOfWork.VatSetting.AddAsync(existing);
                }
                else
                {
                    await _unitOfWork.VatSetting.UpdateAsync(existing);
                }

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "VAT settings saved by user {UserId}. " +
                    "Registered: {IsVatRegistered}",
                    existing.UpdatedByUserId,
                    existing.IsVatRegistered);

                TempData["SuccessMessage"] =
                    "VAT settings saved successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(
                    ex, "Database error while saving VAT settings.");

                ModelState.AddModelError(
                    string.Empty,
                    "The VAT settings could not be saved. " +
                    "Please try again.");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Unexpected error while saving VAT settings.");

                ModelState.AddModelError(
                    string.Empty,
                    "An unexpected error occurred.");
            }

            return View(model);
        }
    }
}
