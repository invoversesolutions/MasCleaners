using MasCleaners.Interfaces;
using MasCleaners.Models;
using MasCleaners.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MasCleaners.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private const string CartCookieName = "MAS_CART";

        public HomeController(
            ILogger<HomeController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }
        // =========================================================
        // HOME
        // =========================================================

        public async Task<IActionResult> Index()
        {
            try
            {
                _logger.LogInformation(
                    "Loading service categories for public home page.");

                var categories =
                    await _unitOfWork.ServiceCategory
                        .GetAllAsync(
                            includeProperties: "ServiceOptions");

                var model = categories
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(c => new HomeServiceCategoryVM
                    {
                        Id = c.Id,

                        Name = c.Name,

                        Description = c.Description,

                        Icon = c.Icon,
                        ImageUrl= c.ImageUrl,

                        StartingPrice = c.ServiceOptions
                            .Where(o => o.IsActive)
                            .Select(o => o.Price)
                            .DefaultIfEmpty(0)
                            .Min(),

                        ServiceOptionCount = c.ServiceOptions
                            .Count(o => o.IsActive)

                    })
                    .ToList();

                _logger.LogInformation(
                    "Loaded {CategoryCount} service categories for public home page.",
                    model.Count);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service categories for public home page.");

                TempData["ErrorMessage"] =
                    "Unable to load services.";

                return View(new List<HomeServiceCategoryVM>());
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult TermsAndConditions()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }
        // =========================================================
        // SERVICE OPTIONS
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Options(int serviceCategoryId)
        {
            _logger.LogInformation(
                "Opening service options for ServiceCategoryId: {ServiceCategoryId}",
                serviceCategoryId);

            if (serviceCategoryId <= 0)
            {
                _logger.LogWarning(
                    "Invalid ServiceCategoryId supplied: {ServiceCategoryId}",
                    serviceCategoryId);

                return BadRequest();
            }

            try
            {
                // =================================================
                // GET CATEGORY
                // =================================================

                var category =
                    await _unitOfWork.ServiceCategory.GetAsync(
                        x => x.Id == serviceCategoryId);

                if (category == null)
                {
                    _logger.LogWarning(
                        "Service category {ServiceCategoryId} was not found.",
                        serviceCategoryId);

                    return NotFound();
                }


                // =================================================
                // GET SERVICE OPTIONS
                // =================================================

                var options =
                    await _unitOfWork.ServiceOptions.GetAllAsync(
                        x =>
                            x.ServiceCategoryId == serviceCategoryId &&
                            x.IsActive);


                // =================================================
                // GET CART
                // =================================================

                // Replace this with however your current cart is identified.
                var cart = await GetOrCreateCartAsync();


                int cartItemCount = 0;
                decimal cartTotal = 0;


                if (cart != null)
                {
                    var cartItems =
                        await _unitOfWork.CartItem.GetAllAsync(
                            x => x.CartId == cart.Id);


                    cartItemCount = cartItems.Sum(x => x.Quantity);

                    cartTotal = cartItems.Sum(
                        x => x.UnitPrice * x.Quantity);
                }


                // =================================================
                // BUILD VIEW MODEL
                // =================================================

                var model = new ServiceOptionsVM
                {
                    ServiceCategory = category,
                    ServiceOptions = options,
                    CartItemCount = cartItemCount,
                    CartTotal = cartTotal
                };


                _logger.LogInformation(
                    "Loaded {OptionCount} service options for category {ServiceCategoryId}. Cart contains {CartItemCount} items.",
                    options.Count(),
                    serviceCategoryId,
                    cartItemCount);


                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading service options for category {ServiceCategoryId}.",
                    serviceCategoryId);

                TempData["ErrorMessage"] =
                    "Unable to load the service options.";

                return RedirectToAction(
                    "Index",
                    "Home");
            }
        }
        // =========================================================
        // GET OR CREATE CART
        // =========================================================

        private async Task<Cart> GetOrCreateCartAsync()
        {
            var guestToken =
                Request.Cookies[CartCookieName];


            // =====================================================
            // EXISTING GUEST CART
            // =====================================================

            if (!string.IsNullOrWhiteSpace(guestToken))
            {
                var existingCart =
                    await _unitOfWork.Cart.GetAsync(x =>
                        x.GuestToken == guestToken &&
                        x.IsActive, includeProperties: "CartItems,CartItems.ServiceOption");

                if (existingCart != null)
                {
                    return existingCart;
                }
            }


            // =====================================================
            // CREATE NEW GUEST CART
            // =====================================================

            guestToken =
                Guid.NewGuid().ToString("N");


            var cart = new Cart
            {
                GuestToken = guestToken,

                IsActive = true,

                CreatedDate =
                    DateTime.Now,

                UpdatedDate =
                    DateTime.Now
            };


            await _unitOfWork.Cart.AddAsync(cart);

            await _unitOfWork.CommitAsync();


            // =====================================================
            // STORE CART TOKEN IN COOKIE
            // =====================================================

            Response.Cookies.Append(
                CartCookieName,
                guestToken,
                new CookieOptions
                {
                    HttpOnly = true,

                    Secure = true,

                    SameSite =
                        SameSiteMode.Lax,

                    Expires =
                        DateTimeOffset.Now.AddDays(30),

                    IsEssential = true
                });


            _logger.LogInformation(
                "Created new guest Cart {CartId}.",
                cart.Id);


            return cart;
        }


    }
}
