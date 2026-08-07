using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Access()
        {
            return View();
        }
    }
}
