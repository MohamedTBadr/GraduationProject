namespace Domain.Entities
{
    public class ServiceImage
    {
        public Guid Id { get; set; }
        public Service Service { get; set; }
        public Guid ServiceId { get; set; }

        public string ImagePath { get; set; }
    }
}