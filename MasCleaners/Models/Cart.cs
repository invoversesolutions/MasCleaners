namespace MasCleaners.Models
{
    public class Cart
    {
        public int Id { get; set; }

        // Used for customers who are not logged in
        public string? GuestToken { get; set; }

        // Will be populated when a customer logs in/registers
        public string? UserId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;

        public ICollection<CartItem> CartItems { get; set; }
            = new List<CartItem>();
    }
}
