namespace MasCleaners.Models
{
    public class Service
    {
        public int Id { get; set; }

        public int ServiceCategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsPopular { get; set; }

        public int DisplayOrder { get; set; }

        public ServiceCategory ServiceCategory { get; set; } = null!;

        public ICollection<ServiceOption> Options { get; set; }
            = new List<ServiceOption>();

        public ICollection<ServiceAddOn> ServiceAddOns { get; set; }
            = new List<ServiceAddOn>();
    }
}
