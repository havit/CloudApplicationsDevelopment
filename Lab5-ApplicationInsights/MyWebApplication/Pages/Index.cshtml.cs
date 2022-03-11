using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MyWebApplication.Pages
{
	public class IndexModel : PageModel
	{
		// TODO: Hodnoty by bylo správné dát do konfigurace. Pro přehlednost a mít vše na jednom místě za účelem studia, nechávám tuto zjednodušenou variantu.
		private const string AzureStorageConnectionString = "DefaultEndpointsProtocol=https;AccountName=...;AccountKey=...;EndpointSuffix=core.windows.net";
		private const string AzureStorageContainerName = "mycontainer";

		public List<BlobItem>? Blobs { get; set; }

		public void OnGet()
		{
			Blobs = new List<BlobItem>();
			//Blobs = CreateClient().GetBlobs().ToList();
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
			return new Azure.Storage.Blobs.BlobContainerClient(AzureStorageConnectionString, AzureStorageContainerName);
		}
	}
}