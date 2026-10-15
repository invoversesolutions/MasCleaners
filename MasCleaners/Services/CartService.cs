using MasCleaners.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace MasCleaners.Services
{
    public class CartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CartService> _logger;

        private const string CartCookieName = "MAS_CART";

        public CartService(
            IUnitOfWork unitOfWork,
            ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // =====================================================
        // MIGRATE GUEST CART TO AUTHENTICATED CUSTOMER
        // =====================================================

        public async Task MigrateGuestCartAsync(
            string userId,
            HttpRequest request,
            HttpResponse response)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException(
                    "A valid authenticated user ID is required.",
                    nameof(userId));
            }

            var guestToken = request.Cookies[CartCookieName];

            if (string.IsNullOrWhiteSpace(guestToken))
            {
                return;
            }

            // =====================================================
            // FIND THE GUEST CART
            // =====================================================

            var guestCart = await _unitOfWork.Cart.GetAsync(
                cart => cart.GuestToken == guestToken
                        && cart.UserId == null
                        && cart.IsActive,
                includeProperties: "CartItems");

            if (guestCart == null)
            {
                // The cookie may refer to an expired or inactive cart.
                response.Cookies.Delete(CartCookieName);
                return;
            }

            // =====================================================
            // FIND THE CUSTOMER'S ACTIVE CART
            // =====================================================

            var userCart = await _unitOfWork.Cart.GetAsync(
                cart => cart.UserId == userId && cart.IsActive,
                includeProperties: "CartItems");

            // =====================================================
            // TRANSFER THE GUEST CART DIRECTLY IF NO USER CART
            // EXISTS
            // =====================================================

            if (userCart == null)
            {
                guestCart.UserId = userId;
                guestCart.GuestToken = null;
                guestCart.UpdatedDate = DateTime.Now;

                await _unitOfWork.Cart.UpdateAsync(guestCart);
                await _unitOfWork.CommitAsync();

                response.Cookies.Delete(CartCookieName);

                _logger.LogInformation(
                    "Guest cart {CartId} was transferred to user {UserId}.",
                    guestCart.Id,
                    userId);

                return;
            }

            // =====================================================
            // MERGE GUEST ITEMS INTO THE EXISTING USER CART
            // =====================================================

            foreach (var guestItem in guestCart.CartItems.ToList())
            {
                var matchingItem = userCart.CartItems.FirstOrDefault(
                    item => item.ServiceOptionId == guestItem.ServiceOptionId);

                if (matchingItem != null)
                {
                    matchingItem.Quantity += guestItem.Quantity;

                    // Recalculate using the existing customer cart's
                    // unit price rather than trusting guest-cart totals.
                    matchingItem.TotalPrice =
                        matchingItem.Quantity * matchingItem.UnitPrice;

                    matchingItem.UpdatedDate = DateTime.Now;

                    await _unitOfWork.CartItem.UpdateAsync(matchingItem);
                }
                else
                {
                    var newItem = new Models.CartItem
                    {
                        CartId = userCart.Id,
                        ServiceOptionId = guestItem.ServiceOptionId,
                        Quantity = guestItem.Quantity,
                        UnitPrice = guestItem.UnitPrice,
                        TotalPrice = guestItem.Quantity * guestItem.UnitPrice,
                        CreatedDate = DateTime.Now,
                        UpdatedDate = DateTime.Now
                    };

                    await _unitOfWork.CartItem.AddAsync(newItem);
                }

                await _unitOfWork.CartItem.RemoveAsync(guestItem);
            }

            // =====================================================
            // DEACTIVATE THE OLD GUEST CART
            // =====================================================

            guestCart.IsActive = false;
            guestCart.UpdatedDate = DateTime.Now;

            await _unitOfWork.Cart.UpdateAsync(guestCart);

            userCart.UpdatedDate = DateTime.Now;

            await _unitOfWork.Cart.UpdateAsync(userCart);

            // One SaveChanges call persists the merge operations.
            await _unitOfWork.CommitAsync();

            // Clear the guest cookie only after persistence succeeds.
            response.Cookies.Delete(CartCookieName);

            _logger.LogInformation(
                "Guest cart {GuestCartId} was merged into cart {UserCartId} for user {UserId}.",
                guestCart.Id,
                userCart.Id,
                userId);
        }
    }
}