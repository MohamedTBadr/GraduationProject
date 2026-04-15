namespace Application.Interfaces
{
    public interface IAdminAIService
    {
        Task<string> GetSalesInsightAsync(string period);
        Task<string> GetAnomalyReportAsync();
        Task<string> ChatWithDataAsync(string question);
        IAsyncEnumerable<string> StreamChatAsync(string question);
    }
}