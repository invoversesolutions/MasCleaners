using MasCleaners.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class BookingController : Controller
    {
        private readonly ILogger<BookingController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public BookingController(ILogger<BookingController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Booking()
        {
            return View();
        }
    }
}
