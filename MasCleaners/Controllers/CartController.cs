using MasCleaners.Interfaces;
using MasCleaners.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace MasCleaners.Controllers
{
    public class CartController : Controller
    {
        private readonly ILogger<CartController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        private const string CartCookieName = "MAS_CART";

        public CartController(
            ILogger<CartController> logger,
            IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }


        // =========================================================
        // CART INDEX
        // =========================================================

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();

                var cartItems = await _unitOfWork.CartItem
                    .GetAllAsync(x => x.CartId == cart.Id);

                return View(cartItems);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error loading shopping cart.");

                TempData["ErrorMessage"] =
                    "Unable to load your cart.";

                return View(new List<CartItem>());
            }
        }


        // =========================================================
        // ADD TO CART
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int serviceOptionId,int quantity = 1)
        {
            if (quantity <= 0)
            {
                quantity = 1;
            }

            try
            {
                var serviceOption =
                    await _unitOfWork.ServiceOptions
                        .GetAsync(x =>
                            x.Id == serviceOptionId &&
                            x.IsActive);

                if (serviceOption == null)
                {
                    _logger.LogWarning(
                        "Attempted to add unavailable ServiceOption {ServiceOptionId} to cart.",
                        serviceOptionId);

                    return NotFound();
                }


                var cart = await GetOrCreateCartAsync();


                var existingItem =
                    await _unitOfWork.CartItem.GetAsync(x =>
                        x.CartId == cart.Id &&
                        x.ServiceOptionId == serviceOptionId);


                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;

                    existingItem.TotalPrice =
                        existingItem.UnitPrice *
                        existingItem.Quantity;

                    existingItem.UpdatedDate =
                        DateTime.Now;

                    await _unitOfWork.CartItem
                        .UpdateAsync(existingItem);
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,

                        ServiceOptionId =
                            serviceOption.Id,

                        Quantity = quantity,

                        UnitPrice =
                            serviceOption.Price,

                        TotalPrice =
                            serviceOption.Price * quantity,

                        CreatedDate =
                            DateTime.Now,

                        UpdatedDate =
                            DateTime.Now
                    };

                    await _unitOfWork.CartItem
                        .AddAsync(cartItem);
                }


                cart.UpdatedDate = DateTime.Now;

                await _unitOfWork.Cart.UpdateAsync(cart);

                await _unitOfWork.CommitAsync();


                _logger.LogInformation(
                    "ServiceOption {ServiceOptionId} added to Cart {CartId}.",
                    serviceOptionId,
                    cart.Id);


                TempData["SuccessMessage"] =
                    "Service added to your cart.";


                return RedirectToAction("Cart", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error adding ServiceOption {ServiceOptionId} to cart.",
                    serviceOptionId);

                TempData["ErrorMessage"] =
                    "Unable to add the service to your cart.";

                return RedirectToAction("Cart","Cart");
            }
        }


        // =========================================================
        // REMOVE FROM CART
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int cartItemId)
        {
            try
            {
                var cart = await GetOrCreateCartAsync();

                var item =
                    await _unitOfWork.CartItem.GetAsync(x =>
                        x.Id == cartItemId &&
                        x.CartId == cart.Id);

                if (item == null)
                {
                    return NotFound();
                }


                await _unitOfWork.CartItem
                    .RemoveAsync(item);


                cart.UpdatedDate =
                    DateTime.Now;

                await _unitOfWork.Cart.UpdateAsync(cart);

                await _unitOfWork.CommitAsync();


                _logger.LogInformation(
                    "CartItem {CartItemId} removed from Cart {CartId}.",
                    cartItemId,
                    cart.Id);


                TempData["SuccessMessage"] =
                    "Service removed from your cart.";

                return RedirectToAction("Cart", "Cart");
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error removing CartItem {CartItemId}.",
                    cartItemId);

                TempData["ErrorMessage"] =
                    "Unable to remove the service.";

                return RedirectToAction("Cart", "Cart");
            }
        }
        // =========================================================
        // INCREASE CART ITEM QUANTITY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseQuantity(int cartItemId)
        {
            try
            {
                var cartItem =
                    await _unitOfWork.CartItem.GetAsync(
                        x => x.Id == cartItemId);

                if (cartItem == null)
                {
                    _logger.LogWarning(
                        "Cart item {CartItemId} was not found while increasing quantity.",
                        cartItemId);

                    return NotFound();
                }

                cartItem.Quantity++;

                await _unitOfWork.CartItem.UpdateAsync(cartItem);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Increased quantity for CartItem {CartItemId} to {Quantity}.",
                    cartItemId,
                    cartItem.Quantity);

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error increasing quantity for CartItem {CartItemId}.",
                    cartItemId);

                TempData["ErrorMessage"] =
                    "Unable to update the cart.";

                return RedirectToAction(nameof(Cart));
            }
        }
        // =========================================================
        // DECREASE CART ITEM QUANTITY
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseQuantity(int cartItemId)
        {
            try
            {
                var cartItem =
                    await _unitOfWork.CartItem.GetAsync(
                        x => x.Id == cartItemId);

                if (cartItem == null)
                {
                    _logger.LogWarning(
                        "Cart item {CartItemId} was not found while decreasing quantity.",
                        cartItemId);

                    return NotFound();
                }

                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;

                    await _unitOfWork.CartItem.UpdateAsync(cartItem);

                    await _unitOfWork.CommitAsync();

                    _logger.LogInformation(
                        "Decreased quantity for CartItem {CartItemId} to {Quantity}.",
                        cartItemId,
                        cartItem.Quantity);
                }
                else
                {
                    _logger.LogInformation(
                        "CartItem {CartItemId} is already at minimum quantity of 1.",
                        cartItemId);
                }

                return RedirectToAction(nameof(Cart));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error decreasing quantity for CartItem {CartItemId}.",
                    cartItemId);

                TempData["ErrorMessage"] =
                    "Unable to update the cart.";

                return RedirectToAction(nameof(Cart));
            }
        }
        // =========================================================
        // CLEAR CART
        // =========================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            try
            {
                var cart = await GetOrCreateCartAsync();

                var items =
                    await _unitOfWork.CartItem
                        .GetAllAsync(x =>
                            x.CartId == cart.Id);

                foreach (var item in items)
                {
                    await _unitOfWork.CartItem
                        .RemoveAsync(item);
                }


                cart.UpdatedDate =
                    DateTime.Now;

                await _unitOfWork.Cart.UpdateAsync(cart);

                await _unitOfWork.CommitAsync();


                _logger.LogInformation(
                    "Cart {CartId} cleared.",
                    cart.Id);


                TempData["SuccessMessage"] =
                    "Your cart has been cleared.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error clearing cart.");

                TempData["ErrorMessage"] =
                    "Unable to clear your cart.";

                return RedirectToAction(nameof(Index));
            }
        }

        // =========================================================
        // GET CART
        // =========================================================
        [HttpGet]
        public async Task<IActionResult> Cart()
        {
            var cart = await GetOrCreateCartAsync();

           
            return View(cart);
        }
        // =========================================================
        // GET OR CREATE CART
        // =========================================================

        private async Task<Cart> GetOrCreateCartAsync()
        {
            // =====================================================
            // CHECK AUTHENTICATION
            // =====================================================

            var userId = User.FindFirstValue(
                System.Security.Claims.ClaimTypes.NameIdentifier);

            var isAuthenticated =
                User.Identity?.IsAuthenticated == true &&
                !string.IsNullOrWhiteSpace(userId);


            // =====================================================
            // AUTHENTICATED CUSTOMER CART
            // =====================================================

            if (isAuthenticated)
            {
                var existingUserCart =
                    await _unitOfWork.Cart.GetAsync(
                        x => x.UserId == userId && x.IsActive,
                        includeProperties:
                            "CartItems,CartItems.ServiceOption");

                if (existingUserCart != null)
                {
                    return existingUserCart;
                }


                // =================================================
                // CREATE CART FOR LOGGED-IN CUSTOMER
                // =================================================

                var userCart = new Cart
                {
                    UserId = userId,

                    GuestToken = null,

                    IsActive = true,

                    CreatedDate = DateTime.Now,

                    UpdatedDate = DateTime.Now
                };

                await _unitOfWork.Cart.AddAsync(userCart);

                await _unitOfWork.CommitAsync();

                _logger.LogInformation(
                    "Created cart {CartId} for user {UserId}.",
                    userCart.Id,
                    userId);

                return userCart;
            }


            // =====================================================
            // ANONYMOUS GUEST CART
            // =====================================================

            var guestToken = Request.Cookies[CartCookieName];

            if (!string.IsNullOrWhiteSpace(guestToken))
            {
                var existingGuestCart =
                    await _unitOfWork.Cart.GetAsync(
                        x => x.GuestToken == guestToken
                             && x.UserId == null
                             && x.IsActive,
                        includeProperties:
                            "CartItems,CartItems.ServiceOption");

                if (existingGuestCart != null)
                {
                    return existingGuestCart;
                }
            }


            // =====================================================
            // CREATE NEW GUEST CART
            // =====================================================

            guestToken = Guid.NewGuid().ToString("N");

            var guestCart = new Cart
            {
                GuestToken = guestToken,

                UserId = null,

                IsActive = true,

                CreatedDate = DateTime.Now,

                UpdatedDate = DateTime.Now
            };

            await _unitOfWork.Cart.AddAsync(guestCart);

            await _unitOfWork.CommitAsync();


            // =====================================================
            // STORE GUEST TOKEN IN COOKIE
            // =====================================================

            Response.Cookies.Append(
                CartCookieName,
                guestToken,
                new CookieOptions
                {
                    HttpOnly = true,

                    Secure = true,

                    SameSite = SameSiteMode.Lax,

                    Expires = DateTimeOffset.Now.AddDays(30),

                    IsEssential = true
                });

            _logger.LogInformation(
                "Created guest cart {CartId}.",
                guestCart.Id);

            return guestCart;
        }
       

    }
}