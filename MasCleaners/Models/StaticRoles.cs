namespace MasCleaners.Models
{
    public static class StaticRoles
    {
        // ======================================================
        // SYSTEM
        // ======================================================

        public const string Role_Super = "Super";
        public const string Role_Admin = "Admin";
        public const string Role_Customer = "Customer";
        public const string Role_Compliance = "Compliance";

        // ======================================================
        // ALL ROLES
        // ======================================================

        public static readonly string[] AllRoles =
        {
            Role_Super,
            Role_Admin,
            Role_Customer,
            Role_Compliance
        };
    }
}
