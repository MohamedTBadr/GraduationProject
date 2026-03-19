namespace Domain.Entities
{
    public class ProductImage
    {
        public Guid Id { get; set; }
        public Product Product { get; set; }
        public Guid ProductId { get; set; }

        public string ImagePath { get; set; }
    }
}