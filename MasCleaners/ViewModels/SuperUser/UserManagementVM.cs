
namespace MasCleaners.ViewModels.SuperUser
{
    public class UserManagementVM
    {
        public string SearchTerm { get; set; } = string.Empty;

        public int TotalUsers { get; set; }

        public int ConfirmedUsers { get; set; }

        public int UnconfirmedUsers { get; set; }

        public List<ManagedUserVM> Users { get; set; } = new();
    }

    public class ManagedUserVM
    {
        public string Id { get; set; } = string.Empty;

        public string? FullName { get; set; }

        public string? Email { get; set; }

        public string? PhoneNumber { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsLockedOut { get; set; }
        public bool IsCurrentUser { get; set; }

        public List<string> Roles { get; set; } = new();
    }
}
