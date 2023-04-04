using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace OCR;

public static class Program
{
    // DOC: https://docs.microsoft.com/cs-cz/azure/cognitive-services/computer-vision/quickstarts/csharp-print-text

    // Replace <Subscription Key> with your valid subscription key.
    const string subscriptionKey = "<<YOUR SUBSCRIPTION KEY>>";

    // You must use the same Azure region in your REST API method as you used to
    // get your subscription keys. For example, if you got your subscription keys
    // from the West US region, replace "westcentralus" in the URL
    // below with "westus".
    //
    // Free trial subscription keys are generated in the "westus" region.
    // If you use a free trial subscription key, you shouldn't need to change
    // this region.
    const string uriBase = "https://westeurope.api.cognitive.microsoft.com/vision/v2.0/ocr";

    public static void Main()
    {
        // Get the path and filename to process from the user.
        Console.WriteLine("Optical Character Recognition:");
        Console.Write("Enter the path to an image with text you wish to read: ");
        string imageFilePath = Console.ReadLine();

        if (File.Exists(imageFilePath))
        {
            // Call the REST API method.
            Console.WriteLine("\nWait a moment for the results to appear.\n");
            MakeOCRRequest(imageFilePath).Wait();
        }
        else
        {
            Console.WriteLine("\nInvalid file path");
        }
        Console.WriteLine("\nPress Enter to exit...");
        Console.ReadLine();
    }

    /// <summary>
    /// Gets the text visible in the specified image file by using
    /// the Computer Vision REST API.
    /// </summary>
    /// <param name="imageFilePath">The image file with printed text.</param>
    private static async Task MakeOCRRequest(string imageFilePath)
    {
        try
        {
            HttpClient client = new();

            // Request headers.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            // Request parameters. 
            // The language parameter doesn't specify a language, so the 
            // method detects it automatically.
            // The detectOrientation parameter is set to true, so the method detects and
            // and corrects text orientation before detecting text.
            string requestParameters = "language=unk&detectOrientation=true";

            // Assemble the URI for the REST API method.
            string uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            // Read the contents of the specified local image
            // into a byte array.
            byte[] byteData = GetImageAsByteArray(imageFilePath);

            // Add the byte array as an octet stream to the request body.
            using (ByteArrayContent content = new ByteArrayContent(byteData))
            {
                // This example uses the "application/octet-stream" content type.
                // The other content types you can use are "application/json"
                // and "multipart/form-data".
                content.Headers.ContentType =
                    new MediaTypeHeaderValue("application/octet-stream");

                // Asynchronously call the REST API method.
                response = await client.PostAsync(uri, content);
            }

            // Asynchronously get the JSON response.
            string contentString = await response.Content.ReadAsStringAsync();

            // Display the JSON response.
            Console.WriteLine("\nResponse:\n\n{0}\n", JsonPrettify(contentString).ToString());
        }
        catch (Exception e)
        {
            Console.WriteLine("\n" + e.Message);
        }
    }

    /// <summary>
    /// Returns the contents of the specified file as a byte array.
    /// </summary>
    /// <param name="imageFilePath">The image file to read.</param>
    /// <returns>The byte array of the image data.</returns>
    private static byte[] GetImageAsByteArray(string imageFilePath)
    {
        // Open a read-only file stream for the specified file.
        using FileStream fileStream = new(imageFilePath, FileMode.Open, FileAccess.Read);
        // Read the file's contents into a byte array.
        using BinaryReader binaryReader = new(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    private static string JsonPrettify(string json)
    {
        //return json;
        using StringReader stringReader = new(json);
        using StringWriter stringWriter = new();
        var jsonReader = new JsonTextReader(stringReader);
        var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
        jsonWriter.WriteToken(jsonReader);
        return stringWriter.ToString();
    }
}