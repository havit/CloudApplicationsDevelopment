using System.Text.Json;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;

namespace Chat;

public static class Example04_StructuredOutput
{
    public static async Task CallAsync()
    {
        AzureKeyCredential credential = new AzureKeyCredential(Config.WestEuropeKey);
        AzureOpenAIClient azureClient = new(new Uri(Config.WestEuropeEndpoint), credential);
        ChatClient client = azureClient.GetChatClient(Config.WestEuropeDeploymentName);

        List<ChatMessage> messages =
        [
            new UserChatMessage("Solve the first order differential equation y' = x^3 + 3. Try to write the final result using plain ASCII."),
        ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "math_reasoning",
                jsonSchema: BinaryData.FromBytes("""
                                                 {
                                                     "type": "object",
                                                     "properties": {
                                                     "steps": {
                                                         "type": "array",
                                                         "items": {
                                                         "type": "object",
                                                         "properties": {
                                                             "explanation": { "type": "string" },
                                                             "output": { "type": "string" }
                                                         },
                                                         "required": ["explanation", "output"],
                                                         "additionalProperties": false
                                                         }
                                                     },
                                                     "final_answer": { "type": "string" }
                                                     },
                                                     "required": ["steps", "final_answer"],
                                                     "additionalProperties": false
                                                 }
                                                 """u8.ToArray()),
                jsonSchemaIsStrict: true)
        };

        ChatCompletion completion = await client.CompleteChatAsync(messages, options);

        using JsonDocument structuredJson = JsonDocument.Parse(completion.Content[0].Text);

        Console.WriteLine($"Final answer: {structuredJson.RootElement.GetProperty("final_answer")}");
        Console.WriteLine("Reasoning steps:");

        foreach (JsonElement stepElement in structuredJson.RootElement.GetProperty("steps").EnumerateArray())
        {
            Console.WriteLine($"  - Explanation: {stepElement.GetProperty("explanation")}");
            Console.WriteLine($"    Output: {stepElement.GetProperty("output")}");
        }
    }
}