using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Chat;
using System.ClientModel;
using System.ComponentModel;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;


namespace AiIntroduction;


public  static class Example07_Tools
{

    public static string modelId = "gpt-4o";

    public static async Task RunAsync()
    {
        Console.WriteLine($"{nameof(Example07_Tools)} is running...");

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
                    Tools = [
                        AIFunctionFactory.Create(GetAge)
                    ]
                });

            //history.Add(new ChatMessage(ChatRole.Assistant, response.Text)); // takto je do history předána pouze výsledná zpráva z AI
            history.AddMessages(response); // takto je do history předána kompletní konverzace, včetně nástrojů, které AI použila


            Console.WriteLine($"[{response.ModelId}]: {response.Text}");
        }
    }

    // Pro ověření chování zadejte prompt: "Seřaď Pavla, Tomáše a Janu podle věku sestupně." nebo "Kolik je Tomášovi let?" nebo "Kdo je nejmladší, Tomáš nebo Pavel?" atd.
    [Description("Metoda pro zjištění věku dle jména.")]
    public static int GetAge([Description("Jméno osoby, jejíž věk chceme zjistit.")] string name)
    {
        // pro ověření, že se nástroj skutečně volá a pro jaký argument
        Console.WriteLine($"[Tool-GetAge] Getting age for {name}");

        return name switch
        {
            "Tomáš" => 25,
            "Pavel" => 30,
            "Jana" => 17,
            _ => throw new ArgumentException("Neznámé jméno", nameof(name))
        };
    }
}
