using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTOs
{
    public class SalesSummaryDto
    {
        public string Period { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }
        public decimal RevenueGrowthPercent { get; set; }
        public List<string> TopProducts { get; set; }
        public Dictionary<string, decimal> RevenueByCategory { get; set; }
        public Dictionary<string, decimal> RevenueByDay { get; set; }
        public List<TopVendorDto> TopVendors { get; set; }
    }

    public class TopVendorDto
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public decimal Revenue { get; set; }
        public int TotalOrders { get; set; }
    }
}
