using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;


namespace AiIntroduction;


public  static class Example06_Introductions
{

    public static string modelId = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example06_Introductions)} is running...");

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

        var history = new List<ChatMessage>();

        while (true)
        {
            Console.Write("[Q]: ");
            message = Console.ReadLine();

            if (string.IsNullOrEmpty(message))
            {
                break;
            }

            history.Add(new ChatMessage(ChatRole.User, message));

            var response = await client.GetResponseAsync(history,
                new ChatOptions
                {
                    ModelId = modelId,
                    Instructions = "Pokud se uživatel jménem Tomáš zeptá na věk, odpověz mu, že je mu 25 let. V ostatních případech, odpověz pravdivě."
                });

            history.AddMessages(response);

            Console.WriteLine($"[{response.ModelId}]: {response.Text}");
        }
    }
}
