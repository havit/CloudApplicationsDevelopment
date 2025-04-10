using System.ClientModel;
using System.Diagnostics.CodeAnalysis;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Assistants;
using OpenAI.Files;
using OpenAI.Images;

namespace Chat;

public static class Example06_Assistent
{
	[Experimental("OPENAI001")]
	public static async Task CallAsync()
	{
		AzureKeyCredential credential = new AzureKeyCredential(Config.EastUsKey);
		AzureOpenAIClient azureClient = new(new Uri(Config.EastUsEndpoint), credential);
		OpenAIFileClient? fileClient = azureClient.GetOpenAIFileClient();

		await using Stream document = BinaryData.FromBytes(
			"""
			{
			   "description": "This document contains the sale history data for Contoso products.",
			   "sales": [
			       {
			           "month": "January",
			           "by_product": {
			               "113043": 15,
			               "113045": 12,
			               "113049": 2
			           }
			       },
			       {
			           "month": "February",
			           "by_product": {
			               "113045": 22
			           }
			       },
			       {
			           "month": "March",
			           "by_product": {
			               "113045": 16,
			               "113055": 5
			           }
			       }
			   ]
			}
			"""u8.ToArray()).ToStream();
		
		OpenAIFile salesFile = await fileClient.UploadFileAsync(
			document,
			"monthly_sales.json",
			FileUploadPurpose.Assistants);
		
		AssistantCreationOptions assistantOptions = new()
		{
			Name = "Assistant346",
			Instructions =
				"You are an assistant that looks up sales data and helps visualize the information based"
				+ " on user queries. When asked to generate a graph, chart, or other visualization, use"
				+ " the code interpreter tool to do so.",
			Tools =
			{
				new CodeInterpreterToolDefinition()
			},
			// ToolResources = new (){"code_interpreter":{"file_ids":["assistant-QYC2m4mX8tP3og1jhM6TKX"]}}
			ToolResources = new ()
			{
				CodeInterpreter = new CodeInterpreterToolResources()
				{
					FileIds = { "assistant-QYC2m4mX8tP3og1jhM6TKX" }
				}
			}
			// ToolResources = new()
			// {
			// 	FileSearch = new()
			// 	{
			// 		NewVectorStores =
			// 		{
			// 			new VectorStoreCreationHelper([salesFile.Id]),
			// 		}
			// 	}
			// },
		};

		AssistantClient assistantClient = azureClient.GetAssistantClient();
		Assistant assistant = await assistantClient.CreateAssistantAsync("gpt-4o", assistantOptions);
		
		ThreadCreationOptions threadOptions = new()
		{
			InitialMessages = { "How well did product 113045 sell in February? Graph its trend over time." }
		};
		ThreadRun threadRun = await assistantClient.CreateThreadAndRunAsync(assistant.Id, threadOptions);
		
		do
		{
			await Task.Delay(TimeSpan.FromSeconds(1));
			threadRun = await assistantClient.GetRunAsync(threadRun.ThreadId, threadRun.Id);
		} while (!threadRun.Status.IsTerminal);
		
		CollectionResult<ThreadMessage> messages = assistantClient.GetMessages(threadRun.ThreadId, 
			new MessageCollectionOptions() { Order = MessageCollectionOrder.Ascending });

		foreach (ThreadMessage message in messages)
		{
			Console.Write($"[{message.Role.ToString().ToUpper()}]: ");
			foreach (MessageContent contentItem in message.Content)
			{
				if (!string.IsNullOrEmpty(contentItem.Text))
				{
					Console.WriteLine($"{contentItem.Text}");

					if (contentItem.TextAnnotations.Count > 0)
					{
						Console.WriteLine();
					}

					// Include annotations, if any.
					foreach (TextAnnotation annotation in contentItem.TextAnnotations)
					{
						if (!string.IsNullOrEmpty(annotation.InputFileId))
						{
							Console.WriteLine($"* File citation, file ID: {annotation.InputFileId}");
						}
						if (!string.IsNullOrEmpty(annotation.OutputFileId))
						{
							Console.WriteLine($"* File output, new file ID: {annotation.OutputFileId}");
						}
					}
				}
				if (!string.IsNullOrEmpty(contentItem.ImageFileId))
				{
					OpenAIFile imageInfo = await fileClient.GetFileAsync(contentItem.ImageFileId);
					BinaryData imageBytes = await fileClient.DownloadFileAsync(contentItem.ImageFileId);
					await using FileStream stream = File.OpenWrite($"{imageInfo.Filename}.png");
					await imageBytes.ToStream().CopyToAsync(stream);

					Console.WriteLine($"<image: {imageInfo.Filename}.png>");
				}
			}
			Console.WriteLine();
		}
	}
}