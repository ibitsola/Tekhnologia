// Services/AIService.cs
using OpenAI.Chat;
using Tekhnologia.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Tekhnologia.Services;

public class AIService : IAIService
{
    private readonly ChatClient _chatClient;

    public AIService(IConfiguration cfg)          // <-- inject merged configuration
    {
        // 1️⃣  env-var wins, 2️⃣  then user-secrets / appsettings
        var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
                  ?? cfg["OpenAI:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new Exception("OpenAI API key missing – set OPENAI_API_KEY or OpenAI:ApiKey.");

        _chatClient = new ChatClient("gpt-3.5-turbo", apiKey);
    }

    public async Task<string> GetBusinessCoachingResponse(string userMessage)
    {
        var messages = new List<ChatMessage>
        {
            new SystemChatMessage("You are an AI business coach offering expert guidance."),
            new UserChatMessage(userMessage)
        };

        var completion = (await _chatClient.CompleteChatAsync(messages)).Value;

        var text = string.Join(" ",
            completion.Content.Select(p => p.Text).Where(t => !string.IsNullOrWhiteSpace(t)));

        return string.IsNullOrWhiteSpace(text)
            ? "⚠️ I couldn’t generate a response."
            : text;
    }
}
