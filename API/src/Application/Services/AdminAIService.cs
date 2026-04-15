//using Application.Interfaces;
//using Domain.Contracts;
//using OllamaSharp;

//namespace Application.Services
//{
//    public class AdminAIService : IAdminAIService
//    {
//        private readonly IOllamaService _ai;
//        //private readonly IRepository _sales;
//        private readonly IOrderRepository _orders;
//        //private readonly IDashboardRepository _dashboard;

//        public AdminAIService(
//            IOllamaService ai,
//            //ISalesRepository sales,
//            IOrderRepository orders,
//            //IDashboardRepository dashboard
//            )
//        {
//            _ai = ai;
//            //_sales = sales;
//            _orders = orders;
//            //_dashboard = dashboard;
//        }

//        public async Task<string> GetSalesInsightAsync(string period)
//        {
//            //var data = await _sales.GetSummaryAsync(period);
//            var prompt = PromptBuilder.SalesInsight(data);
//            return await _ai.GenerateAsync(prompt);
//        }

//        public async Task<string> GetAnomalyReportAsync()
//        {
//            var orders = await _orders.GetLast24HoursAsync();
//            var prompt = PromptBuilder.DetectAnomalies(orders);
//            return await _ai.GenerateAsync(prompt);
//        }

//        public async Task<string> ChatWithDataAsync(string question)
//        {
//            var data = await _dashboard.GetSummaryAsync();
//            var prompt = PromptBuilder.ChatWithData(question, data);
//            return await _ai.GenerateAsync(prompt);
//        }

//        public async IAsyncEnumerable<string> StreamChatAsync(string question)
//        {
//            var data = await _dashboard.GetSummaryAsync();
//            var prompt = PromptBuilder.ChatWithData(question, data);

//            await foreach (var chunk in _ai.StreamAsync(prompt))
//                yield return chunk;
//        }
//    }

//}
