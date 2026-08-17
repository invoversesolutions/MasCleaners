namespace MasCleaners.Models
{
    public class ServiceOption
    {
        public int Id { get; set; }

        public int ServiceId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; }

        public Service Service { get; set; } = null!;
    }
}
