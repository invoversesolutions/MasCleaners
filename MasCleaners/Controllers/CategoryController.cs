using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public CategoryController(
            ILogger<CategoryController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        // =========================================================
        // INDEX
        // =========================================================

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation(
                    "Loading service categories.");

                var categories = await _unitOfWork.ServiceCategory.GetAllAsync();

                return View(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "An error occurred while loading service categories.");

                TempData["ErrorMessage"] =
                    "Unable to load service categories.";

                return View(new List<ServiceCategory>());
            }
        }


        // =========================================================
        // CREATE - GET
        // =========================================================

        [HttpGet]
        public IActionResult Create()
        {
            _logger.LogInformation(
                "Opening create category page.");

            return View();
        }


        // =========================================================
        // CREATE - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCategory model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state while creating category.");

                return View(model);
            }

            try
            {
                _logger.LogInformation(
                    "Creating service category: {CategoryName}",
                    model.Name);

                //model.CreatedDate = DateTime.UtcNow;

                await _unitOfWork.ServiceCategory.AddAsync(model);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Service category created successfully. CategoryId: {CategoryId}",
                    model.Id);

                TempData["SuccessMessage"] =
                    "Category created successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating service category: {CategoryName}",
                    model.Name);

                ModelState.AddModelError(
                    "",
                    "An error occurred while creating the category.");

                return View(model);
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
                    "Loading category {CategoryId} for editing.",
                    id);

                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(p=>p.Id == id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryId} was not found.",
                        id);

                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading category {CategoryId} for editing.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the category.";

                return RedirectToAction(nameof(Index));
            }
        }


        // =========================================================
        // EDIT - POST
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ServiceCategory model)
        {
            if (id != model.Id)
            {
                _logger.LogWarning(
                    "Category ID mismatch. Route ID: {RouteId}, Model ID: {ModelId}",
                    id,
                    model.Id);

                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Invalid model state while editing category {CategoryId}.",
                    id);

                return View(model);
            }

            try
            {
                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(p=>p.Id ==id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryId} was not found during update.",
                        id);

                    return NotFound();
                }

                category.Name = model.Name;
                category.Description = model.Description;
                category.DisplayOrder = model.DisplayOrder;
                category.Icon = model.Icon;
                category.IsActive = model.IsActive;

               await _unitOfWork.ServiceCategory.UpdateAsync(category);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Category {CategoryId} updated successfully.",
                    id);

                TempData["SuccessMessage"] =
                    "Category updated successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating category {CategoryId}.",
                    id);

                ModelState.AddModelError(
                    "",
                    "An error occurred while updating the category.");

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
                    "Loading category {CategoryId} for deletion.",
                    id);

                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(p=>p.Id == id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryId} was not found for deletion.",
                        id);

                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading category {CategoryId} for deletion.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the category.";

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
                    "Attempting to delete category {CategoryId}.",
                    id);

                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(p=>p.Id == id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryId} was not found during deletion.",
                        id);

                    return NotFound();
                }
                category.IsActive = false;
              await  _unitOfWork.ServiceCategory.UpdateAsync(category);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Category {CategoryId} deleted successfully.",
                    id);

                TempData["SuccessMessage"] =
                    "Category deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deleting category {CategoryId}.",
                    id);

                TempData["ErrorMessage"] =
                    "An error occurred while deleting the category.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}