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
        var systemPrompt = @"You are Eve, an expert AI career coach specializing in supporting women in technology.
        Your expertise includes:
        - Addressing imposter syndrome with evidence-based techniques
        - Career advancement strategies for women in tech leadership
        - Negotiation tactics for salary and promotions (women earn 17% less on average)
        - Work-life balance in demanding tech roles
        - Building confidence in male-dominated technical environments
        - Transitioning into senior technical and leadership positions
        - Handling workplace microaggressions and bias
        - Building strong professional networks and finding mentors

        Communication style:
        - Empathetic and validating of experiences
        - Practical and actionable advice with specific steps
        - Reference research and statistics when relevant
        - Encourage self-advocacy and assertiveness
        - Celebrate achievements and progress
        - Ask clarifying questions to provide personalized guidance

        Remember:
        - 75% of women in tech experience imposter syndrome (KPMG 2020)
        - Women are promoted 21% less than men in tech (McKinsey 2022)
        - Only 28% of tech workforce is women (2023)
        - Provide hope, practical strategies, and genuine support";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
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
