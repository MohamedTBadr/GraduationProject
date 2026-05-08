using OpenAI.Chat;
using Shared.Exceptions;

namespace Application.Services.Helpers;

public class LlamaService(ChatClient _llamaClient)
{
    public async Task<Result<string>> SendMessageAsync(string prompt, string? systemPrompt = null)
    {
        if (string.IsNullOrWhiteSpace(prompt))
            return Result<string>.Validation(4001, "Prompt cannot be empty.");

        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new SystemChatMessage(systemPrompt));

        messages.Add(new UserChatMessage(prompt));

        ChatCompletion response;

        try
        {
            response = await _llamaClient.CompleteChatAsync(messages);
        }
        catch (Exception ex)
        {
            return Result<string>.Unexpected(5001, $"Request to Llama failed: {ex.Message}");
        }

        // 1. Check finish reason
        return response.FinishReason switch
        {
            ChatFinishReason.ContentFilter
                => Result<string>.BusinessRule(4221, "Request was blocked by the content safety filter."),

            ChatFinishReason.Length
                => Result<string>.Unexpected(5002, "Response was cut off due to max token limit."),

            ChatFinishReason.Stop => ExtractContent(response),

            _ => Result<string>.Unexpected(5003, $"Model did not finish successfully. Finish Reason: {response.FinishReason}")
        };
    }

    // ── Conversation (multi-turn) ────────────────────────────
    public async Task<Result<string>> SendConversationAsync(
        List<ChatMessage> history,
        string newMessage)
    {
        if (string.IsNullOrWhiteSpace(newMessage))
            return Result<string>.Validation(4001, "Message cannot be empty.");

        history.Add(new UserChatMessage(newMessage));

        ChatCompletion response;

        try
        {
            response = await _llamaClient.CompleteChatAsync(history);
        }
        catch (Exception ex)
        {
            return Result<string>.Unexpected(5001, $"Request to Llama failed: {ex.Message}");
        }

        var result = ExtractContent(response);

        // Append assistant reply to history for next turn
        if (result.IsSuccess)
            history.Add(new AssistantChatMessage(result.Value));

        return result;
    }

    // ── Private Helpers ──────────────────────────────────────
    private static Result<string> ExtractContent(ChatCompletion response)
    {
        if (response.Content == null || response.Content.Count == 0)
            return Result<string>.Unexpected(5004, "The model returned no content.");

        var text = response.Content[0].Text;

        if (string.IsNullOrWhiteSpace(text))
            return Result<string>.Unexpected(5005, "The model returned an empty response.");

        return Result<string>.Success(text);
    }
}