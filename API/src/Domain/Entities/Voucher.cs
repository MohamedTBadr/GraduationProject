namespace Domain.Entities;

public class Voucher
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OwnerId { get; set; }           // user who earned it
    public ApplicationUser Owner { get; set; }

    public string Code { get; set; }            // e.g. "REWARD-A3FK9ZBX"
    public decimal DiscountPercent { get; set; } = 5;
    public bool IsUsed { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);
}