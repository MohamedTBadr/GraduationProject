using Shared.Exceptions;
using Google.GenAI;
using Google.GenAI.Types;


namespace Application.Services.Helpers;


public class GeminiService(Client _geminiClient)
{
  

   
    public async Task<string?> SendMessageAsync(string prompt)
    {
        // ... initial checks ...

        var response = await _geminiClient.Models.GenerateContentAsync(
            model: "gemini-2.5-flash",
            contents: prompt
        );

        // --- Safety and Content Check ---

        // 1. Check if the response was blocked by safety settings or generated no candidates
        if (response.Candidates == null || response.Candidates.Count == 0)
        {
            // Check for rejection reason, if available
            var reason = response.PromptFeedback?.BlockReason.ToString() ?? "Unknown";
            throw new GeminiException($"Request failed or was blocked by the safety system. Reason: {reason}.");
        }

        // 2. Check if the primary candidate contains valid text parts
        var candidate = response.Candidates[0];

        if (candidate.Content == null || candidate.Content.Parts.Count == 0 || string.IsNullOrEmpty(candidate.Content.Parts[0].Text))
        {
            // The candidate may have been blocked or just returned no content
            var finishReason = candidate.FinishReason.ToString();
            throw new GeminiException($"The model did not generate a usable response. Finish Reason: {finishReason}");
        }

        // --- Successful Extraction ---
        var generatedText =candidate.Content.Parts[0].Text;

        return generatedText;
    }
}