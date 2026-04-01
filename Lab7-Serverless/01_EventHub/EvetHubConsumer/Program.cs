using Azure.Messaging.EventHubs.Consumer;
using EventHub.Config;

namespace EvetHubConsumer
{
    internal class Program
	{
        public static async Task Main(string[] args)
        {
            await MainAsync(args);
        }

        private static async Task MainAsync(string[] args)
        {
            const string consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
            var consumer = new EventHubConsumerClient(consumerGroup, Config.EventHubConnectionString, Config.EventHubName);
            await using (consumer.ConfigureAwait(false))
            {
                try
                {
                    await ReadEventFromEventHubAsync(consumer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                finally
                {
                    await consumer.CloseAsync();
                }
            }
        }

        private static async Task ReadEventFromEventHubAsync(EventHubConsumerClient consumer)
        {
            var firstPartition = (await consumer.GetPartitionIdsAsync()).First();
            var startingPosition = EventPosition.Earliest;

            var options = new ReadEventOptions
            {
                TrackLastEnqueuedEventProperties = true
            };

            await foreach (var @event in consumer.ReadEventsFromPartitionAsync(firstPartition, startingPosition, options))
            {
                Console.WriteLine("Received new event: ");
                Console.WriteLine($"\tSequence number: {@event.Data.SequenceNumber}");
                Console.WriteLine($"\tPartition id: {@event.Partition.PartitionId}");
                Console.WriteLine($"\tBody: {@event.Data.EventBody}");
                Console.WriteLine();
            }
        }
    }
}