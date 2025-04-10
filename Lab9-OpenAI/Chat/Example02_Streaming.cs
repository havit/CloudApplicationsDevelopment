using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Chat;

public static class Example02_Streaming
{
	public static Task CallAsync()
	{
		AzureKeyCredential credential = new AzureKeyCredential(Config.WestEuropeKey);
		AzureOpenAIClient azureClient = new(new Uri(Config.WestEuropeEndpoint), credential);
		ChatClient chatClient = azureClient.GetChatClient(Config.WestEuropeDeploymentName);
		
		var messages = new List<ChatMessage>()
		{
			new SystemChatMessage("You are a helpful assistant."),
			new UserChatMessage("I am going to Paris, what should I see?"),
		};

		var response = chatClient.CompleteChatStreaming(messages);

		WriteResponse(response);
		
		while (true)
		{
			messages.Add(Console.ReadLine());
		
			// Create the chat completion request
			response = chatClient.CompleteChatStreaming(messages);

			WriteResponse(response);
		}
	}

	private static void WriteResponse(CollectionResult<StreamingChatCompletionUpdate> response)
	{
		foreach (StreamingChatCompletionUpdate update in response)
		{
			foreach (ChatMessageContentPart updatePart in update.ContentUpdate)
			{
				Console.Write(updatePart.Text);
			}
		}
		Console.WriteLine("");
	}
}