using Microsoft.AspNetCore.Mvc;

namespace MasCleaners.Controllers
{
    public class LegalController : Controller
    {
        [HttpGet]
        public IActionResult PrivacyPolicy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult TermsAndConditions()
        {
            return View();
        }
    }
}