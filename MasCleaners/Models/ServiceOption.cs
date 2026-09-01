namespace MasCleaners.Models
{
    public class ServiceOption
    {
        public int Id { get; set; }

        public int ServiceCategoryId { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public bool IsActive { get; set; }

        public int DisplayOrder { get; set; }

        public DateTime CreatedDate { get; set; }

        public ServiceCategory? ServiceCategory { get; set; }
    }
}
