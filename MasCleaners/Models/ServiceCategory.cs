namespace MasCleaners.Models
{
    public class ServiceCategory
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? Icon { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }
        public DateTime CreatedData { get; set; } = DateTime.Now;

        public ICollection<Service> Services { get; set; }
            = new List<Service>();
    }
}
