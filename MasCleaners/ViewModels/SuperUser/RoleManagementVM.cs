
namespace MasCleaners.ViewModels.SuperUser
{
    public class RoleManagementVM
    {
        public int TotalRoles { get; set; }

        public int RolesWithoutUsers { get; set; }

        public List<ManagedRoleVM> Roles { get; set; } = new();
    }

    public class ManagedRoleVM
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public int UserCount { get; set; }

        public bool IsSystemRole { get; set; }
    }
}
