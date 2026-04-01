using Azure;
using Azure.AI.OpenAI;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OpenAI.Chat;
using System.Text.Json;

namespace ServiceBusChat;

/// <summary>
/// Service Bus chat s AI botem.
///
/// Spuštění jako lidský účastník:
///   dotnet run
///
/// Spuštění jako AI bot (reaguje na zprávy začínající "@ai"):
///   dotnet run -- --bot
/// </summary>
internal class Program
{
    private static readonly Guid CurrentId = Guid.NewGuid();
    private static string? _currentName;

	private static async Task Main(string[] args)
    {
        if (args.Contains("--bot"))
        {
            await RunAsBotAsync();
        }
        else
        {
            await RunAsHumanAsync();
        }
    }

    // -------------------------------------------------------------------------
    // Lidský účastník
    // -------------------------------------------------------------------------

	private static async Task RunAsHumanAsync()
    {
        Console.Write("Ahoj! Zadej svoji přezdívku: ");
        _currentName = Console.ReadLine() ?? "Neznámý";
        Console.WriteLine("Tip: Napiš '@ai <otázka>' a AI asistent odpoví.");
        Console.WriteLine("Prázdný řádek = ukončení.\n");

        var adminClient = new ServiceBusAdministrationClient(Config.ServiceBusConnectionString);
        var subscriptionName = $"chat-{CurrentId}";

        await adminClient.CreateSubscriptionAsync(Config.TopicName, subscriptionName);
        Console.WriteLine($"[Připojen jako {_currentName}]\n");

        try
        {
            await Task.WhenAll(
                ReceiveMessagesAsync(subscriptionName, HumanMessageHandler),
                SendMessagesAsync()
            );
        }
        finally
        {
            await adminClient.DeleteSubscriptionAsync(Config.TopicName, subscriptionName);
            Console.WriteLine("\n[Odpojeno]");
        }
    }

	private static async Task SendMessagesAsync()
    {
        await using var client = new ServiceBusClient(Config.ServiceBusConnectionString);
        var sender = client.CreateSender(Config.TopicName);

        Console.Write($"{_currentName}: ");
        while (true)
        {
            var body = Console.ReadLine();

            if (string.IsNullOrEmpty(body))
                break;

            await SendChatMessageAsync(sender, CurrentId, _currentName!, body);
            Console.Write($"{_currentName}: ");
        }
    }

	private static async Task HumanMessageHandler(ProcessMessageEventArgs args)
    {
        var message = DeserializeMessage(args.Message.Body.ToString());

        if (message is null || message.Id == CurrentId)
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        // Vymaž aktuální řádek (smaže rozepsaný prompt i případný rozepsaný text)
        // \r = začátek řádku, \x1b[K = ANSI: vymaž do konce řádku
        Console.Write("\r\x1b[K");

        if (message.Name == "🤖 AI Asistent")
		{
			WriteColored($"{message.Name}: {message.Body}", ConsoleColor.Cyan);
		}
		else
		{
			Console.WriteLine($"{message.Name}: {message.Body}");
		}

		// Znovu zobraz prompt – po smazání řádku uživatel nevidí, kam psát
        Console.Write($"{_currentName}: ");

        await args.CompleteMessageAsync(args.Message);
    }

    // -------------------------------------------------------------------------
    // AI bot
    // -------------------------------------------------------------------------

	private static async Task RunAsBotAsync()
    {
        _currentName = "🤖 AI Asistent";
        Console.WriteLine($"{_currentName} spuštěn. Čekám na zprávy začínající '@ai'...");
        Console.WriteLine("Ukončení: Ctrl+C\n");

        // Bot poslouchá na vlastní subscription – dostane kopii každé zprávy
        await ReceiveMessagesAsync(Config.AiBotSubscription, BotMessageHandler);
    }

	private static async Task BotMessageHandler(ProcessMessageEventArgs args)
    {
        var message = DeserializeMessage(args.Message.Body.ToString());

        // Ignoruj vlastní zprávy a zprávy jiných botů
        if (message is null || message.Id == CurrentId || message.Name == "🤖 AI Asistent")
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        // Reaguj pouze na zprávy začínající "@ai"
        if (!message.Body.StartsWith("@ai", StringComparison.OrdinalIgnoreCase))
        {
            await args.CompleteMessageAsync(args.Message);
            return;
        }

        var question = message.Body["@ai".Length..].Trim();
        Console.WriteLine($"📩 [{message.Name}]: {question}");

        var aiResponse = await GetAiResponseAsync(question);
        Console.WriteLine($"💬 Odpovídám: {aiResponse}\n");

        await using var client = new ServiceBusClient(Config.ServiceBusConnectionString);
        var sender = client.CreateSender(Config.TopicName);
        await SendChatMessageAsync(sender, CurrentId, "🤖 AI Asistent", $"[{message.Name}] {aiResponse}");

        await args.CompleteMessageAsync(args.Message);
    }

	private static async Task<string> GetAiResponseAsync(string question)
    {
        try
        {
            var azureClient = new AzureOpenAIClient(
                new Uri(Config.AzureOpenAiEndpoint),
                new AzureKeyCredential(Config.AzureOpenAiApiKey));

            var chatClient = azureClient.GetChatClient(Config.AzureOpenAiDeployment);

            var completion = await chatClient.CompleteChatAsync(
            [
                new SystemChatMessage("Jsi přátelský AI asistent v skupinovém chatu. Odpovídej stručně (max 2 věty) a česky."),
                new UserChatMessage(question)
            ]);

            return completion.Value.Content[0].Text;
        }
        catch (Exception ex)
        {
            return $"(Chyba: {ex.Message[..Math.Min(60, ex.Message.Length)]})";
        }
    }

    // -------------------------------------------------------------------------
    // Sdílené pomocné metody
    // -------------------------------------------------------------------------

	private static async Task ReceiveMessagesAsync(string subscription, Func<ProcessMessageEventArgs, Task> handler)
    {
        await using var client = new ServiceBusClient(Config.ServiceBusConnectionString);
        var processor = client.CreateProcessor(Config.TopicName, subscription, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += handler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync();
        await Task.Delay(Timeout.Infinite); // běží dokud se program neukončí
    }

	private static async Task SendChatMessageAsync(ServiceBusSender sender, Guid id, string name, string body)
    {
        var chatMessage = new ChatMessage(id, name, body);
        var serialized = JsonSerializer.Serialize(chatMessage);
        await sender.SendMessageAsync(new ServiceBusMessage(serialized));
    }

	private static ChatMessage? DeserializeMessage(string body)
    {
        try { return JsonSerializer.Deserialize<ChatMessage>(body); }
        catch { return null; }
    }

	private static Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine($"❌ Chyba: {args.Exception.Message}");
        return Task.CompletedTask;
    }

	private static void WriteColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

internal record ChatMessage(Guid Id, string Name, string Body);
