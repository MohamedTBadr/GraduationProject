//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Text.Json;
//using System.Threading.Tasks;

//namespace Application
//{
//    public static class PromptBuilder
//    {
//        // Admin: Sales insights
//        public static string SalesInsight(SalesSummaryDto data) => $"""
//        You are a business analyst AI. Analyze this sales data and provide insights.
        
//        Period: {data.Period}
//        Total Revenue: {data.TotalRevenue:C}
//        Total Orders: {data.TotalOrders}
//        Top Products: {string.Join(", ", data.TopProducts)}
//        Revenue by Category: {JsonSerializer.Serialize(data.RevenueByCategory)}
        
//        Provide:
//        1. Key performance highlights
//        2. Areas of concern
//        3. Actionable recommendations
        
//        Be concise and business-focused.
//        """;

//        // Admin: Anomaly detection
//        public static string DetectAnomalies(List<OrderDto> recentOrders) => $"""
//        You are a fraud detection AI. Review these recent orders and flag suspicious activity.
        
//        Orders (last 24h):
//        {JsonSerializer.Serialize(recentOrders)}
        
//        Flag anything suspicious such as:
//        - Unusually large orders
//        - Multiple orders from same IP
//        - Orders from new accounts with high value
//        - Unusual locations or patterns
        
//        Return a JSON array of flagged order IDs with reasons.
//        """;

//        // Vendor: Inventory insights
//        public static string InventoryInsight(VendorInventoryDto data) => $"""
//        You are an inventory management AI for a vendor dashboard.
        
//        Vendor: {data.VendorName}
//        Total Products: {data.TotalProducts}
//        Low Stock Items: {JsonSerializer.Serialize(data.LowStockItems)}
//        Best Sellers: {JsonSerializer.Serialize(data.BestSellers)}
//        Dead Stock (no sales 30+ days): {JsonSerializer.Serialize(data.DeadStock)}
        
//        Provide specific recommendations to improve inventory turnover.
//        """;

//        // Chat with data
//        public static string ChatWithData(string userQuestion, object contextData) => $"""
//        You are a helpful business intelligence assistant with access to the following data:
        
//        {JsonSerializer.Serialize(contextData)}
        
//        User question: {userQuestion}
        
//        Answer based only on the data provided. If the answer isn't in the data, say so.
//        Be concise and clear.
//        """;
//    }
//}
