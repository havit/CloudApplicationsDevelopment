using System.ClientModel;
using Azure;
using Azure.AI.OpenAI;
using OpenAI;
using OpenAI.Chat;
using OpenAI.Images;

namespace Chat;

public static class Example05_ImageGeneration
{
	public static async Task CallAsync()
	{
		AzureKeyCredential credential = new AzureKeyCredential(Config.EastUsKey);
		AzureOpenAIClient azureClient = new(new Uri(Config.EastUsEndpoint), credential);
		ImageClient client = azureClient.GetImageClient(Config.EastUsImageDeploymentName);
		
		string prompt = "Vygeneruj menší počítačovou učebnu s asi 12 studenty, která je z polovny plná, " +
		                "na matematicko fyzikální fakultě za počítači, kteří nadšeně čekají na výsledek, " +
		                "který se vygeneruje. Pohled bude od učitele. Vygeneruj obrázek s lehkým nádechem scifi.";

		ImageGenerationOptions options = new()
		{
			Quality = GeneratedImageQuality.High,
			Size = GeneratedImageSize.W1024xH1024,
			Style = GeneratedImageStyle.Vivid,
			ResponseFormat = GeneratedImageFormat.Bytes,
		};

		Console.WriteLine("Generating image...");
		GeneratedImage image = await client.GenerateImageAsync(prompt, options);
		BinaryData bytes = image.ImageBytes;

		var filename = $"{Guid.NewGuid()}.png";
		await using FileStream stream = File.OpenWrite(filename);
		await bytes.ToStream().CopyToAsync(stream);
		Console.WriteLine(filename);
	}
}