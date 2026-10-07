using Microsoft.AspNetCore.Identity;

namespace MasCleaners.Models
{
    public class ApplicationUser : IdentityUser
    {
        // ======================================================
        // CUSTOMER
        // ======================================================

        public Customer? Customer { get; set; }
    }
}
