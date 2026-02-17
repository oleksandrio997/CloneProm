namespace CloneProm.Models
{
    // DTO for storing cart items in session
    public class SessionCartItem
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
