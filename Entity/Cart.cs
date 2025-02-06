namespace CartService.Entity
{
    public class Cart
    {
        public int CustomerId { get; set; }
        public int BookId { get; set; }
        public int Quantity { get; set; } = 1; // Default quantity is 1
    }
}
