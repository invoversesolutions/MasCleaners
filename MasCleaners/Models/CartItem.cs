namespace MasCleaners.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        // Cart
        public int CartId { get; set; }

        public Cart Cart { get; set; } = null!;


        // Selected service option
        public int ServiceOptionId { get; set; }

        public ServiceOption ServiceOption { get; set; } = null!;


        // Quantity
        public int Quantity { get; set; } = 1;


        // Price captured when added to cart
        public decimal UnitPrice { get; set; }

        public decimal TotalPrice { get; set; }


        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime UpdatedDate { get; set; } = DateTime.Now;
    }
}
