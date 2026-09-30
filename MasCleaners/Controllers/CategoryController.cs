using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ILogger<CategoryController> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _environment;

        public CategoryController(
            ILogger<CategoryController> logger,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _environment = environment;
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
        public async Task<IActionResult> Create(ServiceCategory model, IFormFile? imageFile)
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
                // =====================================================
                // IMAGE UPLOAD
                // =====================================================

                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "services");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    string extension =
                        Path.GetExtension(imageFile.FileName)
                        .ToLowerInvariant();

                    string fileName =
                        $"{Guid.NewGuid()}{extension}";

                    string filePath =
                        Path.Combine(uploadsFolder, fileName);

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    // Save absolute URL
                    model.ImageUrl =
                        $"{Request.Scheme}://{Request.Host}/images/services/{fileName}";
                }
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
        public async Task<IActionResult> Edit(
            int id,
            ServiceCategory model,
            IFormFile? imageFile)
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
                    await _unitOfWork.ServiceCategory
                        .GetAsync(p => p.Id == id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Category {CategoryId} was not found during update.",
                        id);

                    return NotFound();
                }

                // =====================================================
                // UPDATE BASIC INFORMATION
                // =====================================================

                category.Name = model.Name;
                category.Description = model.Description;
                category.DisplayOrder = model.DisplayOrder;
                category.Icon = model.Icon;
                category.IsActive = model.IsActive;


                // =====================================================
                // IMAGE UPLOAD
                // =====================================================

                if (imageFile != null && imageFile.Length > 0)
                {
                    string uploadsFolder = Path.Combine(
                        _environment.WebRootPath,
                        "images",
                        "services");

                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }


                    // =================================================
                    // DELETE OLD LOCAL IMAGE
                    // =================================================

                    if (!string.IsNullOrWhiteSpace(category.ImageUrl))
                    {
                        try
                        {
                            var oldUri = new Uri(category.ImageUrl);

                            string oldPath = oldUri.LocalPath
                                .TrimStart('/')
                                .Replace(
                                    '/',
                                    Path.DirectorySeparatorChar);

                            string oldFilePath = Path.Combine(
                                _environment.WebRootPath,
                                oldPath);

                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);

                                _logger.LogInformation(
                                    "Deleted old service image for category {CategoryId}: {ImagePath}",
                                    id,
                                    oldFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(
                                ex,
                                "Could not delete old image for category {CategoryId}.",
                                id);
                        }
                    }


                    // =================================================
                    // CREATE NEW FILE NAME
                    // =================================================

                    string extension =
                        Path.GetExtension(imageFile.FileName)
                            .ToLowerInvariant();

                    string fileName =
                        $"{Guid.NewGuid()}{extension}";

                    string filePath =
                        Path.Combine(
                            uploadsFolder,
                            fileName);


                    // =================================================
                    // SAVE IMAGE
                    // =================================================

                    using (var stream = new FileStream(
                        filePath,
                        FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }


                    // =================================================
                    // ABSOLUTE URL
                    // =================================================

                    var request = HttpContext.Request;

                    string baseUrl =
                        $"{request.Scheme}://{request.Host}";

                    category.ImageUrl =
                        $"{baseUrl}/images/services/{fileName}";


                    _logger.LogInformation(
                        "New service image uploaded for category {CategoryId}: {ImageUrl}",
                        id,
                        category.ImageUrl);
                }


                // =====================================================
                // SAVE
                // =====================================================

                await _unitOfWork.ServiceCategory
                    .UpdateAsync(category);

                await _unitOfWork.CommitAsync();


                _logger.LogInformation(
                    "Category {CategoryId} updated successfully.",
                    id);


                TempData["SuccessMessage"] =
                    "Service updated successfully.";


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
                    "An error occurred while updating the service.");

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

        // =========================================================
        // DETAILS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                _logger.LogInformation(
                    "Loading service category {CategoryId} for viewing.",
                    id);

                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(p => p.Id == id);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Service category {CategoryId} was not found.",
                        id);

                    return NotFound();
                }

                return View(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service category {CategoryId}.",
                    id);

                TempData["ErrorMessage"] =
                    "Unable to load the service.";

                return RedirectToAction(nameof(Index));
            }
        }
    }
}