using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace ServiceBusReceiver
{
    internal class Program
    {
        private const string connectionString = ""; // ie. Endpoint=sb://xxx.servicebus.windows.net/;SharedAccessKeyName=xxx;SharedAccessKey=xxx
        private const string queueOrTopicName = "chat"; // add topic
        private const string subscriptionName = ""; // add subscription to topic

        private static Guid CurrentId = Guid.NewGuid();
        private static string? CurrentName;

        static async Task Main(string[] args)
        {
            Console.Write("Ahoj! Zadej svoji přezdívku: ");
            CurrentName = Console.ReadLine() ?? "Neznámý";

            var receiveTask = ReceiveMessagesAsync();
            var sendTask = SendMessagesAsync();

            await Task.WhenAll(receiveTask, sendTask);
        }

        static async Task ReceiveMessagesAsync()
        {
            var client = new ServiceBusClient(connectionString);
            var processor = client.CreateProcessor(queueOrTopicName, subscriptionName, new ServiceBusProcessorOptions());

            processor.ProcessMessageAsync += MessageHandler;
            processor.ProcessErrorAsync += ErrorHandler;

            await processor.StartProcessingAsync();
        }

        static async Task SendMessagesAsync()
        {
            var client = new ServiceBusClient(connectionString);
            var sender = client.CreateSender(queueOrTopicName);

            while (true)
            {
                Console.Write($"{CurrentName}: ");
                var messageBody = Console.ReadLine();

                if (string.IsNullOrEmpty(messageBody))
                {
                    break;
                }

                var serializedMessage = JsonSerializer.Serialize(new Message(CurrentId, CurrentName!, messageBody));
                var message = new ServiceBusMessage(serializedMessage);
                await sender.SendMessageAsync(message);
            }

            await sender.DisposeAsync();
            await client.DisposeAsync();
        }

        static async Task MessageHandler(ProcessMessageEventArgs args)
        {
            string body = args.Message.Body.ToString();
            Message? message;
            try
            {
                message = JsonSerializer.Deserialize<Message>(body);
            }
            catch (Exception)
            {
                return;
            }
            if (message is null || message.Id == CurrentId)
            {
                return;
            }
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.WriteLine($"{message.Name}: {message.Body}");
            Console.Write($"{CurrentName}: ");

            await args.CompleteMessageAsync(args.Message);
        }

        static Task ErrorHandler(ProcessErrorEventArgs args)
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
            return Task.CompletedTask;
        }
    }

    internal record Message(Guid Id, string Name, string Body);
}
