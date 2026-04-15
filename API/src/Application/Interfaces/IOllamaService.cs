namespace Application.Interfaces
{
    public interface IOllamaService
    {
        Task<string> GenerateAsync(string prompt);
    }
}