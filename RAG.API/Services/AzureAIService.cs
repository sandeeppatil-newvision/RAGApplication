using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using OpenAI.Chat;

namespace RAG.API.Services
{
    public class AzureAIService
    {
        private string? endpoint;
        private string? key;

        public AzureAIService(IConfiguration config)
        {
              endpoint = new Uri(config["Azure:OpenAIEndpoint"]).ToString();
              key = config["Azure:OpenAIKey"].ToString();

        }
        public async Task<string> AskQuestionAsync(string context, string question)
        {

            // Retrieve the OpenAI endpoint from environment variables
           // var endpoint = "https://aiwithsandeep.openai.azure.com/";
            if (string.IsNullOrEmpty(endpoint))
            {
                Console.WriteLine("Please set the AZURE_OPENAI_ENDPOINT environment variable.");
                return null;
            }

            if (string.IsNullOrEmpty(key))
            {
                Console.WriteLine("Please set the AZURE_OPENAI_KEY environment variable.");
                return null;
            }

            AzureKeyCredential credential = new AzureKeyCredential(key);

            // Initialize the AzureOpenAIClient
            AzureOpenAIClient azureClient = new(new Uri(endpoint), credential);

            // Initialize the ChatClient with the specified deployment name
            ChatClient chatClient = azureClient.GetChatClient("gpt-4o");

            // Create chat completion request
            ChatCompletion completion1 = chatClient.CompleteChat(
                new List<ChatMessage>()
                {
              new UserChatMessage(question)
                },
                new ChatCompletionOptions
                {
                    // PastMessages = 10,
                    Temperature = (float)0.7,
                    TopP = (float)0.95,
                    FrequencyPenalty = (float)0,
                    PresencePenalty = (float)0,
                    // MaxTokens = 800,
                    // StopSequences = new List<string>(),
                }
            ); 

            // Create a list of chat messages
            var messages = new List<ChatMessage>
    {
        new SystemChatMessage(@"You are an AI assistant. Use the provided document context to answer the user's question."),
        new UserChatMessage($"Document Content:\n{context}"),
              new UserChatMessage(question),
    };


            // Create chat completion options
            var options = new ChatCompletionOptions
            {
                Temperature = (float)0.7,
                MaxOutputTokenCount = 800,

                TopP = (float)0.95,
                FrequencyPenalty = (float)0,
                PresencePenalty = (float)0
            };


            try
            {
                // Create the chat completion request
                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

                // Print the response
                if (completion != null)
                {
                    Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
                    return JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true });
                }
                else
                {
                    Console.WriteLine("No response received.");
                    return "No response received.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return $"An error occurred: {ex.Message}";
            }
        }
    }
}
