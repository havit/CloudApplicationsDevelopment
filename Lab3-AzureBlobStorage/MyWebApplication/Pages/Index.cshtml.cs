using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyWebApplication.Pages
{
	public class IndexModel : PageModel
	{
		// TODO: Hodnoty by bylo správné dát do konfigurace. Pro přehlednost a mít vše na jednom místě za účelem studia, nechávám tuto zjednodušenou variantu.
		private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=azureappdevelopment;AccountKey=HBa5vTZMNe4Udcqq2TMfX0rbBVRGCukmPVm7ytLnP1gCvfLn3IWVLfd2ZcpWa9hlyU2EdT+vK+eZH6AKDwL41Q==;EndpointSuffix=core.windows.net";
		private const string AzureStorageContainerName = "privatecontainer";
		private const string AzureStorageName = "azureappdevelopment";

		public List<BlobItem>? Blobs { get; set; }

		public void OnGet()
		{
			Blobs = CreateClient().GetBlobs().ToList();
		}

		public FileStreamResult OnGetDownloadBlob(string blobName)
		{
			var blobClient = CreateClient().GetBlobClient(blobName);
			var contentType = blobClient.GetProperties().Value.ContentType;
			var stream = blobClient.OpenRead(); // není třeba řešit Dispose - stream nám uzavře infrastruktura Razor Pages.
			return new FileStreamResult(stream, contentType);
		}

		private BlobContainerClient CreateClient()
		{
			string containerEndpoint = $"https://{AzureStorageName}.blob.core.windows.net/{AzureStorageContainerName}";
			return new BlobContainerClient(new Uri(containerEndpoint), new Azure.Identity.DefaultAzureCredential());
		}
	}
}