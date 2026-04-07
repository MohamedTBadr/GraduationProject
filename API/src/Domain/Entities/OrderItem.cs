namespace Domain.Entities
{
    public class OrderItem
    {
        public Guid Id { get; set; }
        public Order Order { get; set; }
        public Guid OrderId { get; set; }
        public Guid VendorId { get; set; }

        public Guid ServiceId { get; set; } = Guid.Empty;
        public string ServiceName { get; set; }= string.Empty;

        public List<string>? PackageItems { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}