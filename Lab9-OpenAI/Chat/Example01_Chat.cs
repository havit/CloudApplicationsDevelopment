using System.Text;
using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Chat;

public static class Example01_Chat
{
	public static async Task CallAsync()
	{
		AzureKeyCredential credential = new AzureKeyCredential(Config.WestEuropeKey);
		AzureOpenAIClient azureClient = new(new Uri(Config.WestEuropeEndpoint), credential);
		ChatClient chatClient = azureClient.GetChatClient(Config.WestEuropeDeploymentName);

		// Create a list of chat messages
		var messages = new List<ChatMessage>
		{
			new SystemChatMessage("Jsi asistent, který říká vtipy v češtině."),
			new UserChatMessage("Řekni vtip o studentech."),
		};

		var options = new ChatCompletionOptions
		{
			Temperature = (float)0.7,
			MaxOutputTokenCount = 800,
			TopP = (float)0.95,
			FrequencyPenalty = (float)0,
			PresencePenalty = (float)0
		};

		// Create the chat completion request
		ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

		Console.WriteLine(JsonSerializer.Serialize(completion, new JsonSerializerOptions() { WriteIndented = true }));
		Console.WriteLine(completion.Content[0].Text);
	}
}