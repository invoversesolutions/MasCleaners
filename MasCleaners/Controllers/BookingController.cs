using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class BookingController : Controller
    {
        public IActionResult Booking()
        {
            return View();
        }
    }
}
