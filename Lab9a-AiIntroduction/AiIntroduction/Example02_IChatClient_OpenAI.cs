using Microsoft.Extensions.AI;


namespace AiIntroduction;


public  static class Example02_IChatClient_OpenAI
{

    public static string modelId = "gpt-4o-mini";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example02_IChatClient_OpenAI)} is running...");

        IChatClient client = new OpenAI.Chat.ChatClient(modelId, Environment.GetEnvironmentVariable("OPENAI_API_KEY")).AsIChatClient();


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
