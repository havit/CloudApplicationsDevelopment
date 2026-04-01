using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using EventHub.Config;

namespace EventHubPublisher
{
    // Source: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
    // More samples: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/eventhub/Azure.Messaging.EventHubs/samples
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        private static async Task MainAsync(string[] args)
        {
            EventHubProducerClient producer = new(Config.EventHubConnectionString, Config.EventHubName);
            await using (producer.ConfigureAwait(false))
            {
                try
                {
                    await SendMessagesToEventHub(producer);
                }
                catch (Exception ex)
                {
                    // Transient failures will be automatically retried as part of the
                    // operation. If this block is invoked, then the exception was either
                    // fatal or all retries were exhausted without a successful publish.
                    Console.WriteLine(ex);
                }
                finally
                {
                    await producer.CloseAsync();
                }
            }
        }

        // Creates an event hub client and sends X messages to the event hub.
        private static async Task SendMessagesToEventHub(EventHubProducerClient producer)
        {
            Console.WriteLine("Zadejte zprávu, kterou chcete odeslat do event hubu a potvrďte klávesou enter.");
            Console.WriteLine("Prázdný řádek = ukončení.");

            while (true)
            {
                var text = Console.ReadLine() ?? "";

                if (string.IsNullOrEmpty(text))
				{
					break;
				}

				// Každá zpráva dostane vlastní batch – jeden batch nelze odeslat vícekrát
                using var eventBatch = await producer.CreateBatchAsync();

                if (!eventBatch.TryAdd(new EventData(text)))
                {
                    Console.WriteLine("Zpráva je příliš velká pro jeden batch.");
                    continue;
                }

                await producer.SendAsync(eventBatch);
                Console.WriteLine("  ✅ Odesláno");
            }
        }
    }
}