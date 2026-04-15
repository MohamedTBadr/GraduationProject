namespace Domain.Entities
{
    public class OrderInsight
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public int OrderCount { get; set; }
        public decimal? LastMonthRevenue { get; set; }
        public decimal PercentageGrowth { get; set; }
    }

}
