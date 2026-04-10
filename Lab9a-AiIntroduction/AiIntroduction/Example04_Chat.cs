using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;


namespace AiIntroduction;


public  static class Example04_Chat
{

    public static string modelId = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example04_Chat)} is running...");

        IChatClient client =
            new ChatClient(
                model: Configuration.DeploymentName,
                credential: new ApiKeyCredential(Configuration.ApiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(Configuration.Endpoint)
                })
            .AsIChatClient();

        string message;

        while (true)
        {
            Console.Write("[Q]: ");
            message = Console.ReadLine();

            if (string.IsNullOrEmpty(message))
            {
                break;
            }

            var response = await client.GetResponseAsync(message,
                new ChatOptions
                {
                    ModelId = modelId,
                });

            Console.WriteLine($"[{response.ModelId}]: {response.Text}");
        }
    }
}
