using Google.GenAI;
using Google.GenAI.Types;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
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
        if (string.IsNullOrEmpty(prompt))
        {
            return BadRequest("Prompt cannot be empty.");
        }

        try
        {
            // Call the generateContent API
            var response = await _geminiClient.Models.GenerateContentAsync(
                model: "gemini-2.5-flash", // Use a model like 'gemini-2.5-flash'
                contents: prompt
            );

            // Extract the generated text from the response
            var generatedText = response.Candidates[0].Content.Parts[0].Text;

            return Ok(generatedText);
        }
        catch (Exception ex)
        {
            // Log the error and return a server error status
            Console.WriteLine($"Error calling Gemini API: {ex.Message}");
            return StatusCode(500, "An error occurred while generating content.");
        }
    }
}