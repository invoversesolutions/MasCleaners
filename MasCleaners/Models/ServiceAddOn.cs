namespace MasCleaners.Models
{
    public class ServiceAddOn
    {
        public int ServiceId { get; set; }

        public int AddOnId { get; set; }

        public Service Service { get; set; } = null!;

        public AddOn AddOn { get; set; } = null!;
    }
}
