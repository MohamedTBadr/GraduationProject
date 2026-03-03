namespace DAL.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }

        public string ProductName { get; set; }= string.Empty;

        public List<string>? PackageItems { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}