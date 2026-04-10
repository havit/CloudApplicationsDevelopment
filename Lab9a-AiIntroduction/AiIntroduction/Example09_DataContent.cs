using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using System.Reflection;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;


namespace AiIntroduction;


public static class Example09_DataContent
{

    public static string modelId = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example09_DataContent)} is running...");

        IChatClient client =
            new ChatClient(
                model: Configuration.DeploymentName,
                credential: new ApiKeyCredential(Configuration.ApiKey),
                options: new OpenAIClientOptions
                {
                    Endpoint = new Uri(Configuration.Endpoint)
                })
            .AsIChatClient()
            .AsBuilder()
            .UseFunctionInvocation()
            .Build();

        string message;

        var history = new List<ChatMessage>();

        string question = "Kolik je na fotce psů, koček a mostů?";

        Console.WriteLine($"[Q]: {question}");

        var assembly = Assembly.GetExecutingAssembly();

        
        using var resourceStream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream("AiIntroduction.Data.AiIntro-pesKocky.png") // Namespace.Složka.NázevSouboru.png
            ?? throw new InvalidOperationException("Embedded resource 'AiIntroduction.AiIntro-pesKocky.png' not found.");

        var imageData = await BinaryData.FromStreamAsync(resourceStream);

        history.Add(new ChatMessage(ChatRole.User,
        [
            new TextContent(question),
            new DataContent(imageData.ToMemory(), "image/png")
        ]));

        ChatResponse<Details> response = await client.GetResponseAsync<Details>(history,
        new ChatOptions
            {
                ModelId = modelId,
            });

        history.AddMessages(response);

        Console.WriteLine($"[{response.ModelId}]: {response.Text}");
    }

    public record Details
    {
        public int NumberOfDogs { get; set; }
        public int NumberOfBridges { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
    }

}
