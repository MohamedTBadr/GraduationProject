namespace DAL.Entities
{
    public class EventItem     
    {
        public Guid Id { get; set; }

        public Event Event { get; set; }
        public Guid EventId { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string VendorName { get; set; }

        public int Quantity { get; set; }





    }
}
