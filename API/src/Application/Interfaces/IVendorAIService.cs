namespace Application.Interfaces
{
    public interface IVendorAIService
    {
        Task<string> GetInventoryInsightAsync(int vendorId);
        Task<string> SummarizeReportAsync(int vendorId);
        Task<string> ChatWithDataAsync(int vendorId, string question);
    }
}