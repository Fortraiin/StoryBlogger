using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using static System.Environment;

public class OpenAIService
{
    private readonly string endpoint;
    private readonly string deploymentName;
    private readonly string apiVersion;
    private readonly string apiKey;

    public OpenAIService(IConfiguration config)
    {
        var section = config.GetSection("AzureOpenAI");
        endpoint = section.GetValue<string>("Endpoint");
        deploymentName = section.GetValue<string>("DeploymentName");
        apiVersion = section.GetValue<string>("ApiVersion");
        apiKey = GetEnvironmentVariable("AZURE_OPENAI_KEY");
    }

    public async Task<string> GenerateStoryAsync(string prompt)
    {
        AzureOpenAIClient client = new (new Uri(endpoint), new AzureKeyCredential(apiKey));
        ChatClient chatClient = client.GetChatClient(deploymentName);

        var requestOptions = new ChatCompletionOptions()
        {
            Temperature = 1,
        };

        List<ChatMessage> messages = new List<ChatMessage>()
        {
            new SystemChatMessage("You are a creative storyteller tasked to look at the available images and write a narrative about them."),
            new UserChatMessage(prompt),
        };

        var response = chatClient.CompleteChat(messages, requestOptions);
        return response.Value.Content[0].Text;
    }
}