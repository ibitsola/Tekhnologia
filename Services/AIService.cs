using OpenAI.Chat;
using System.Threading.Tasks;

namespace Services
{
    public class AIService
    {
        private readonly ChatClient _chatClient;

        public AIService()
        {
            // Retrieve the API key from environment variables (or your secrets)
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Key is missing. Please set OPENAI_API_KEY as an environment variable.");
            }

            // Use a model you have access to, e.g. "gpt-3.5-turbo"
            _chatClient = new ChatClient(model: "gpt-3.5-turbo", apiKey: apiKey);
        }

        public async Task<string> GetBusinessCoachingResponse(string userMessage)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage("You are an AI business coach offering expert guidance."),
                new UserChatMessage(userMessage)
            };

            // Make the API call
            var completionResult = await _chatClient.CompleteChatAsync(messages);

            // Extract the ChatCompletion from the ClientResult
            var chatCompletion = completionResult.Value;
            if (chatCompletion == null)
            {
                return "Error: No response received from the AI service.";
            }

            // chatCompletion.Content is a ChatMessageContent object,
            // which inherits from Collection<ChatMessageContentPart>.
            // So you can iterate directly over chatCompletion.Content:
            var contentCollection = chatCompletion.Content;

            if (contentCollection == null || contentCollection.Count == 0)
            {
                return "I couldn't generate a response.";
            }

            // Each ChatMessageContentPart can contain text (part.Text), images, etc.
            // We'll extract all non-empty text parts and join them.
            var textParts = contentCollection
                .Select(part => part.Text)
                .Where(t => !string.IsNullOrWhiteSpace(t));

            var responseText = string.Join(" ", textParts);

            if (string.IsNullOrWhiteSpace(responseText))
            {
                return "I couldn't generate a response.";
            }

            return responseText;
        }
    }
}
