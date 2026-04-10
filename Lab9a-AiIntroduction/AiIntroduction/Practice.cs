using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;

namespace AiIntroduction;

public static class Practice
{
    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Practice)} is running...");

        // Lokální modely přes Ollama
        //string modelId = "gemma3:latest";
        //IChatClient client = new OllamaApiClient(new Uri("http://localhost:11436"), modelId);

        // OpenAI
        //string modelId = "gpt-4o-mini";
        //IChatClient client = new OpenAI.Chat.ChatClient(model: modelId, Environment.GetEnvironmentVariable("OPENAI_API_KEY"))
        //    .AsIChatClient();

        // OpenAI s využitím Azure OpenAI Service
        string modelId = "gpt-4o";
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
            .UseFunctionInvocation() // Aktivace, že se služba bude chovat jako stavový automat
            .Build();

        var history = new List<ChatMessage>();

        while (true)
        {

            Console.Write($"[Q]: ");
            string message = Console.ReadLine();

            history.Add(new ChatMessage(ChatRole.User, message));

            var result = await client.GetResponseAsync(history,
                new ChatOptions
                {
                    //Instructions = "Pokud se uživatel jménem Tomáš zeptá na věk, odpověz mu, že je mu 25 let. V ostatních případech, odpověz pravdivě.",
                    Tools = [
                        AIFunctionFactory.Create(GetPersonAge)
                    ]
                });

            //history.Add(new ChatMessage(ChatRole.Assistant, result.Text)); // takto je do history předána pouze výsledná zpráva z AI
            history.AddMessages(result); // takto je do history předána kompletní konverzace, včetně nástrojů, které AI použila

            Console.WriteLine($"[{result.ModelId}]: {result.Text}");
        }
    }

    // Pro ověření chování zadejte prompt: "Seřaď Pavla, Tomáše a Janu podle věku sestupně." nebo "Kolik je Tomášovi let?" nebo "Kdo je nejmladší, Tomáš nebo Pavel?" atd.
    [Description("Gets the age of a person by their name.")]
    private static int GetPersonAge([Description("The name of the person whose age is to be retrieved.")] string name)
    {
        // pro ověření, že se nástroj skutečně volá a pro jaký argument
        Console.WriteLine($"[TOOL] GetPersonAge was called with argument: {name}");

        return name switch
        {
            "Pavel" => 30,
            "Tomáš" => 25,
            "Jana" => 17,
            _ => throw new ArgumentException($"Unknown name: {name}", nameof(name))
        };
    }
}
