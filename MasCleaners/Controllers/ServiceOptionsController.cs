using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class ServiceOptionsController : Controller
    {
        private readonly ILogger<ServiceOptionsController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public ServiceOptionsController(
            ILogger<ServiceOptionsController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        // =========================================================
        // INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index(int serviceCategoryId)
        {
            try
            {
                _logger.LogInformation(
                    "Loading service options for ServiceCategoryId: {ServiceCategoryId}",
                    serviceCategoryId);

                var options = await _unitOfWork.ServiceOptions
                    .GetAllAsync(x => x.ServiceCategoryId == serviceCategoryId,includeProperties:"ServiceCategory");

                ViewBag.ServiceCategoryId = serviceCategoryId;

                return View(options);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service options for ServiceCategoryId: {ServiceCategoryId}",
                    serviceCategoryId);

                TempData["ErrorMessage"] =
                    "Unable to load service options.";

                ViewBag.ServiceCategoryId = serviceCategoryId;

                return View(new List<ServiceOption>());
            }
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create(int serviceCategoryId)
        {
            _logger.LogInformation(
                "Opening create service option page for ServiceCategoryId: {ServiceCategoryId}",
                serviceCategoryId);

            var model = new ServiceOption
            {
                ServiceCategoryId = serviceCategoryId,
                IsActive = true
            };

            return View(model);
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceOption model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state while creating service option.");

                return View(model);
            }

            try
            {
                _logger.LogInformation(
                    "Creating service option: {OptionName} for ServiceCategoryId: {ServiceCategoryId}",
                    model.Name,
                    model.ServiceCategoryId);

                model.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.ServiceOptions.AddAsync(model);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Service option created successfully. OptionId: {OptionId}",
                    model.Id);

                TempData["SuccessMessage"] =
                    "Service option created successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        serviceCategoryId = model.ServiceCategoryId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating service option: {OptionName}",
                    model.Name);

                ModelState.AddModelError(
                    "",
                    "An error occurred while creating the service option.");

                return View(model);
            }
        }


        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Loading service option {ServiceOptionId} for details.",
                    id);

                var option =
                    await _unitOfWork.ServiceOptions.GetAsync(
                        x => x.Id == id);

                if (option == null)
                {
                    _logger.LogWarning(
                        "Service option {ServiceOptionId} was not found.",
                        id);

                    return NotFound();
                }

                return View(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service option {ServiceOptionId} for details.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the service option.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // EDIT - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Loading service option {ServiceOptionId} for editing.",
                    id);

                var option =
                    await _unitOfWork.ServiceOptions.GetAsync(
                        x => x.Id == id);

                if (option == null)
                {
                    _logger.LogWarning(
                        "Service option {ServiceOptionId} was not found.",
                        id);

                    return NotFound();
                }

                return View(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service option {ServiceOptionId} for editing.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the service option.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            ServiceOption model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning(
                    "Service option ID mismatch. Route ID: {RouteId}, Model ID: {ModelId}",
                    id,
                    model.Id);

                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state while editing service option {ServiceOptionId}.",
                    id);

                return View(model);
            }

            try
            {
                var option =
                    await _unitOfWork.ServiceOptions.GetAsync(
                        x => x.Id == id);

                if (option == null)
                {
                    _logger.LogWarning(
                        "Service option {ServiceOptionId} was not found during update.",
                        id);

                    return NotFound();
                }

                option.Name = model.Name;
                option.Description = model.Description;
                option.Price = model.Price;
                option.IsActive = model.IsActive;
                option.DisplayOrder = model.DisplayOrder;
                option.ServiceCategoryId = model.ServiceCategoryId;

                await _unitOfWork.ServiceOptions.UpdateAsync(option);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Service option {ServiceOptionId} updated successfully.",
                    id);

                TempData["SuccessMessage"] =
                    "Service option updated successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        serviceCategoryId = option.ServiceCategoryId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating service option {ServiceOptionId}.",
                    id);

                ModelState.AddModelError(
                    "",
                    "An error occurred while updating the service option.");

                return View(model);
            }
        }


        // =========================================================
        // DELETE - GET
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Loading service option {ServiceOptionId} for deletion.",
                    id);

                var option =
                    await _unitOfWork.ServiceOptions.GetAsync(
                        x => x.Id == id);

                if (option == null)
                {
                    _logger.LogWarning(
                        "Service option {ServiceOptionId} was not found for deletion.",
                        id);

                    return NotFound();
                }

                return View(option);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service option {ServiceOptionId} for deletion.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the service option.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // DELETE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Attempting to delete service option {ServiceOptionId}.",
                    id);

                var option =
                    await _unitOfWork.ServiceOptions.GetAsync(
                        x => x.Id == id);

                if (option == null)
                {
                    _logger.LogWarning(
                        "Service option {ServiceOptionId} was not found during deletion.",
                        id);

                    return NotFound();
                }

                var serviceCategoryId =
                    option.ServiceCategoryId;

                option.IsActive = false;

                await _unitOfWork.ServiceOptions.UpdateAsync(option);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Service option {ServiceOptionId} deleted successfully.",
                    id);

                TempData["SuccessMessage"] =
                    "Service option deleted successfully.";

                return RedirectToAction(
                    nameof(Index),
                    new
                    {
                        serviceCategoryId = serviceCategoryId
                    });
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting service option {ServiceOptionId}.",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while deleting the service option.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}