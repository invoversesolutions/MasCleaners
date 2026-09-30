namespace MasCleaners.ViewModels
{
    public class HomeServiceCategoryVM
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string? Description { get; set; }

        public string? Icon { get; set; }
        public string? ImageUrl { get; set; }

        public decimal StartingPrice { get; set; }

        public int ServiceOptionCount { get; set; }
    }
}
