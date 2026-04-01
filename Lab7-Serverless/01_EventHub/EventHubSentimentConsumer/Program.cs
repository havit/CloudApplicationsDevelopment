using Azure;
using Azure.AI.OpenAI;
using Azure.Messaging.EventHubs.Consumer;
using EventHub.Config;
using OpenAI.Chat;
using System.Text.Json;

namespace EventHubSentimentConsumer;

/// <summary>
/// Event Hub Consumer s analýzou sentimentu pomocí OpenAI.
/// Přijaté zprávy jsou barevně zobrazeny podle nálady (pozitivní/negativní/neutrální).
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🎯 Event Hub Sentiment Consumer spuštěn.");
        Console.WriteLine("Čekám na zprávy a analyzuji jejich sentiment...\n");
        Console.WriteLine("Legenda barev:");
        PrintColored("  ✅ POSITIVE – pozitivní zpráva", ConsoleColor.Green);
        PrintColored("  ❌ NEGATIVE – negativní zpráva", ConsoleColor.Red);
        PrintColored("  ⚪ NEUTRAL  – neutrální zpráva", ConsoleColor.Gray);
        Console.WriteLine();

        var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
        var consumer = new EventHubConsumerClient(consumerGroup, Config.EventHubConnectionString, Config.EventHubName);

        await using (consumer.ConfigureAwait(false))
        {
            try
            {
                await ReadEventsWithSentimentAsync(consumer);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Chyba: {ex.Message}");
            }
        }
    }

    static async Task ReadEventsWithSentimentAsync(EventHubConsumerClient consumer)
    {
        var firstPartition = (await consumer.GetPartitionIdsAsync()).First();

        // EventPosition.Latest = čteme jen nové zprávy (poslané po spuštění consumeru)
        var startingPosition = EventPosition.Latest;

        var options = new ReadEventOptions { TrackLastEnqueuedEventProperties = true };

        Console.WriteLine($"Naslouchám na partition: {firstPartition}");
        Console.WriteLine("(Spusťte EventHubPublisher v druhém terminálu a posílejte zprávy)\n");

        await foreach (var @event in consumer.ReadEventsFromPartitionAsync(firstPartition, startingPosition, options))
        {
            var messageText = @event.Data.EventBody.ToString();

            if (string.IsNullOrWhiteSpace(messageText))
			{
				continue;
			}

			Console.WriteLine($"📨 Přijata zpráva #{@event.Data.SequenceNumber}: \"{messageText}\"");
            Console.Write("   🔍 Analyzuji sentiment...");

            SentimentResult sentiment = await AnalyzeSentimentAsync(messageText);

            // Přepsat řádek "Analyzuji..." výsledkem
            Console.SetCursorPosition(0, Console.CursorTop);

            var (emoji, color) = sentiment.Label switch
            {
                "POSITIVE" => ("✅", ConsoleColor.Green),
                "NEGATIVE" => ("❌", ConsoleColor.Red),
                _ => ("⚪", ConsoleColor.Gray)
            };

            PrintColored($"   {emoji} {sentiment.Label} ({sentiment.Score:P0}) – {sentiment.Reason}", color);
            Console.WriteLine();
        }
    }

    static async Task<SentimentResult> AnalyzeSentimentAsync(string text)
    {
        try
        {
            var azureClient = new AzureOpenAIClient(new Uri(Config.AzureOpenAiEndpoint), new AzureKeyCredential(Config.AzureOpenAiApiKey));
            var client = azureClient.GetChatClient(Config.AzureOpenAiDeployment);

            var prompt = $$"""
							Analyzuj sentiment následující zprávy a vrať POUZE platný JSON (bez markdown):
							{"label": "POSITIVE nebo NEGATIVE nebo NEUTRAL", "score": číslo 0.0-1.0, "reason": "krátké vysvětlení česky (max 6 slov)"}

							Zpráva: "{{text}}"
							""";

            ChatCompletion completion = await client.CompleteChatAsync(
            [
                new UserChatMessage(prompt)
            ]);

            var response = completion.Content[0].Text.Trim()
                .Replace("```json", "")
                .Replace("```", "")
                .Trim();

            var result = JsonSerializer.Deserialize<SentimentResult>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return result ?? new SentimentResult("NEUTRAL", 0.5f, "Nepodařilo se analyzovat");
        }
        catch (Exception ex)
        {
            return new SentimentResult("NEUTRAL", 0.5f, $"Chyba analýzy: {ex.Message[..Math.Min(40, ex.Message.Length)]}");
        }
    }

	private static void PrintColored(string text, ConsoleColor color)
    {
        var original = Console.ForegroundColor;
        Console.ForegroundColor = color;
        Console.WriteLine(text);
        Console.ForegroundColor = original;
    }
}

internal record SentimentResult(string Label, float Score, string Reason);
