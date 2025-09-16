using System.Text.Json;

public class OpenAIService
{
    private readonly AzureOpenAISettings _settings;

    public OpenAIService(IConfiguration config)
    {
        _settings = config.GetSection("AzureOpenAI").Get<AzureOpenAISettings>();
    }

    public async Task<string> GenerateStoryAsync(string prompt)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("api-key", _settings.ApiKey);

        var requestBody = new
        {
            messages = new[]
            {
                new { role = "system", content = "You are a creative storyteller." },
                new { role = "user", content = prompt }
            },
            max_tokens = 300,
            temperature = 0.7
        };

        var response = await client.PostAsJsonAsync(
            $"{_settings.Endpoint}openai/deployments/{_settings.DeploymentName}/chat/completions?api-version={_settings.ApiVersion}",
            requestBody
        );

        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        return result.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
    }
}