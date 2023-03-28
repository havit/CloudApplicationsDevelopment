using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;

namespace EventHubPublisher
{
    // Source: https://docs.microsoft.com/en-us/azure/event-hubs/event-hubs-dotnet-standard-getstarted-send
    // More samples: https://github.com/Azure/azure-sdk-for-net/tree/main/sdk/eventhub/Azure.Messaging.EventHubs/samples
    public class Program
    {
        private const string EventHubConnectionString = "Endpoint=sb://mffeventhub.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=nPbqiusz8R8tGnBPQArIaOGVyqEGt4ALc+AEhFhd4Nc="; // ie Endpoint=sb://XXX.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=XXX
        private const string EventHubName = "myeventhub1"; // ie myeventhub1

        public static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        private static async Task MainAsync(string[] args)
        {
            EventHubProducerClient producer = new(EventHubConnectionString, EventHubName);
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

            using EventDataBatch eventBatch = await producer.CreateBatchAsync();

            while (true)
            {
                string text = Console.ReadLine() ?? "";

                var eventData = new EventData(text);
                if (!eventBatch.TryAdd(eventData))
                {
                    // At this point, the batch is full but our last event was not
                    // accepted.  For our purposes, the event is unimportant so we
                    // will intentionally ignore it.  In a real-world scenario, a
                    // decision would have to be made as to whether the event should
                    // be dropped or published on its own.
                    Console.WriteLine("Event is full! Cannot add event.");
                    break;
                }

                // When the producer publishes the event, it will receive an
                // acknowledgment from the Event Hubs service; so long as there is no
                // exception thrown by this call, the service assumes responsibility for
                // delivery.  Your event data will be published to one of the Event Hub
                // partitions, though there may be a (very) slight delay until it is
                // available to be consumed.

                await producer.SendAsync(eventBatch);
            }
        }
    }
}