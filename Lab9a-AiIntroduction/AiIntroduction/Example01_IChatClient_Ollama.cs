using Microsoft.Extensions.AI;
using OllamaSharp;


namespace AiIntroduction;


public  static class Example01_IChatClient_Ollama
{

    public static string modelId = "gemma3:latest";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example01_IChatClient_Ollama)} is running...");

        IChatClient client = new OllamaApiClient("http://localhost:11436");

        string message = "Ahoj, já jsem Tomáš.";

        Console.WriteLine($"[Q]: {message}");

        var response = await client.GetResponseAsync(message,
            new ChatOptions
            {
                ModelId = modelId
            });

        Console.WriteLine(response);

        Console.WriteLine($"[{response.ModelId}]: {response.Text}");

    }
}
