using Microsoft.Extensions.AI;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;


namespace AiIntroduction;


public  static class Example08_StructuredOutput
{

    public static string modelId = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example08_StructuredOutput)} is running...");

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

        string question = "Na kraji Prahy bylo vidět pět psů a sedm koček, jak kráčejí postupně přes tři mosty.";
        Console.WriteLine($"[Q]: {question}");

        ChatResponse<Details> response = await client.GetResponseAsync<Details>(question, 
        new ChatOptions
            {
                ModelId = modelId,
                    
            });

        Console.WriteLine($"[{response.ModelId}]: {response.Text}");
    }

    // Definování struktury, kterou očekáváme jako výstup od AI. AI se bude snažit tento objekt naplnit a vrátit nám ho jako odpověď.
    public record Details
    {
        public int NumberOfDogs { get; set; }
        public int NumberOfBridges { get; set; }
        public string Location { get; set; }
        public string City { get; set; }
    }

}
