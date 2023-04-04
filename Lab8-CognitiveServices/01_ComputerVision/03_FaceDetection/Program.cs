using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace FaceDetection;

public class Program
{
    // DOC: https://docs.microsoft.com/cs-cz/azure/cognitive-services/face/QuickStarts/CSharp

    // Replace <Subscription Key> with your valid subscription key.
    const string subscriptionKey = "<<YOUR SUBSCRIPTION KEY>>";

    // NOTE: You must use the same region in your REST call as you used to
    // obtain your subscription keys. For example, if you obtained your
    // subscription keys from westus, replace "westcentralus" in the URL
    // below with "westus".
    //
    // Free trial subscription keys are generated in the "westus" region.
    // If you use a free trial subscription key, you shouldn't need to change
    // this region.
    const string uriBase = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";

    public static void Main(string[] args)
    {

        // Get the path and filename to process from the user.
        Console.WriteLine("Detect faces:");
        Console.Write("Enter the path to an image with faces that you wish to analyze: ");
        string imageFilePath = Console.ReadLine();

        if (File.Exists(imageFilePath))
        {
            try
            {
                MakeAnalysisRequest(imageFilePath);
                Console.WriteLine("\nWait a moment for the results to appear.\n");
            }
            catch (Exception e)
            {
                Console.WriteLine("\n" + e.Message + "\nPress Enter to exit...\n");
            }
        }
        else
        {
            Console.WriteLine("\nInvalid file path.\nPress Enter to exit...\n");
        }
        Console.ReadLine();
    }

    // Gets the analysis of the specified image by using the Face REST API.
    private static async void MakeAnalysisRequest(string imageFilePath)
    {
        HttpClient client = new();

        // Request headers.
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

        // Request parameters. A third optional parameter is "details".
        string requestParameters = "returnFaceLandmarks=false" +
            "&returnFaceAttributes=headPose,glasses," +
            "occlusion,accessories,blur,exposure,noise";

        // Assemble the URI for the REST API Call.
        string uri = uriBase + "?" + requestParameters;

        HttpResponseMessage response;

        // Request body. Posts a locally stored JPEG image.
        byte[] byteData = GetImageAsByteArray(imageFilePath);

        using ByteArrayContent content = new(byteData);
        // This example uses content type "application/octet-stream".
        // The other content types you can use are "application/json"
        // and "multipart/form-data".
        content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        // Execute the REST API call.
        response = await client.PostAsync(uri, content);

        // Get the JSON response.
        string contentString = await response.Content.ReadAsStringAsync();

        // Display the JSON response.
        Console.WriteLine("\nResponse:\n");
        Console.WriteLine(JsonPrettify(contentString));
        Console.WriteLine("\nPress Enter to exit...");
    }

    // Returns the contents of the specified file as a byte array.
    private static byte[] GetImageAsByteArray(string imageFilePath)
    {
        using FileStream fileStream = new(imageFilePath, FileMode.Open, FileAccess.Read);
        BinaryReader binaryReader = new(fileStream);
        return binaryReader.ReadBytes((int)fileStream.Length);
    }

    private static string JsonPrettify(string json)
    {
        using var stringReader = new StringReader(json);
        using var stringWriter = new StringWriter();

        var jsonReader = new JsonTextReader(stringReader);
        var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Newtonsoft.Json.Formatting.Indented };
        jsonWriter.WriteToken(jsonReader);
        return stringWriter.ToString();
    }
}