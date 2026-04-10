using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;


namespace AiIntroduction;


public  static class Example03_MicrosoftFoundry
{

    public static string modelId = "gpt-4o";
    public static string deploymentName = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example03_MicrosoftFoundry)} is running...");

        IChatClient client =
            new ChatClient(
                model: deploymentName,
                credential: new ApiKeyCredential(Configuration.ApiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(Configuration.Endpoint)
                })
            .AsIChatClient();


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
