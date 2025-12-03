using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GeminiController : ControllerBase
{
    private readonly Client _geminiClient;

    // Inject the Client using Dependency Injection
    public GeminiController(Client geminiClient)
    {
        _geminiClient = geminiClient;
    }

    [HttpGet("generate-text")]
    public async Task<IActionResult> GenerateText(string prompt)
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
            return StatusCode(400, $"Request failed or was blocked by the safety system. Reason: {reason}.");
        }

        // 2. Check if the primary candidate contains valid text parts
        var candidate = response.Candidates[0];

        if (candidate.Content == null || candidate.Content.Parts.Count == 0 || string.IsNullOrEmpty(candidate.Content.Parts[0].Text))
        {
            // The candidate may have been blocked or just returned no content
            var finishReason = candidate.FinishReason.ToString();
            return StatusCode(400, $"The model did not generate a usable response. Finish Reason: {finishReason}");
        }

        // --- Successful Extraction ---
        var generatedText = candidate.Content.Parts[0].Text;

        return Ok(generatedText);
    }
}