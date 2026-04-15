using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateView : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.Sql(@"
                EXEC('CREATE VIEW View_OrderInsights AS
                WITH MonthlyStats AS (
                    -- We aggregate orders by Month and Year
                    SELECT 
                        YEAR(CreatedAt) as [Year],
                        MONTH(CreatedAt) as [Month],
                        SUM(Amount) as MonthlyRevenue,
                        COUNT(Id) as OrderCount
                    FROM Orders
                    WHERE PaymentStatus = ''Success''
                    GROUP BY YEAR(CreatedAt), MONTH(CreatedAt)
                )
                SELECT 
                    curr.[Year],
                    curr.[Month],
                    curr.MonthlyRevenue,
                    curr.OrderCount,
                    ISNULL(prev.MonthlyRevenue, 0) AS LastMonthRevenue,
                    -- Logic to calculate growth/decrease percentage
                    CASE 
                        WHEN prev.MonthlyRevenue IS NULL OR prev.MonthlyRevenue = 0 THEN 100
                        ELSE ((curr.MonthlyRevenue - prev.MonthlyRevenue) / prev.MonthlyRevenue) * 100 
                    END AS PercentageGrowth
                FROM MonthlyStats curr
                LEFT JOIN MonthlyStats prev ON 
                    (curr.[Month] = 1 AND prev.[Month] = 12 AND prev.[Year] = curr.[Year] - 1) OR
                    (curr.[Month] > 1 AND prev.[Month] = curr.[Month] - 1 AND prev.[Year] = curr.[Year]);
                ')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW View_OrderInsights;");
        }
    }
}
