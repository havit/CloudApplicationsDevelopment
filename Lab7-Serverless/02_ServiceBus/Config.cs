namespace ServiceBusChat;

/// <summary>
/// Centrální konfigurace pro Service Bus chat.
/// Vyplňte hodnoty zde – změna se projeví ve všech projektech.
///
/// Azure infrastruktura:
///   - Service Bus Namespace s SKU Standard nebo vyšší (pro Topics)
///   - Topic: "chat"
///   - Subscription: "chat-ai"  (pro AI bota; lidé dostávají dynamické subscription)
/// </summary>
public static class Config
{
    // ===== SERVICE BUS =====
    // Azure Portal → Service Bus Namespace → Shared access policies → connection string
    public const string ServiceBusConnectionString = "<<INSERT YOUR SERVICE BUS CONNECTION STRING>>";
    public const string TopicName = "chat";
	public const string AiBotSubscription = "chat-ai";

    // ===== AZURE OPENAI (Foundry) =====
    // ai.azure.com → váš projekt → Deployments → vyberte model → zkopírujte Endpoint a Key
    public const string AzureOpenAiEndpoint = "<<INSERT YOUR FOUNDRY AI ENDPOINT>>"; // https://xxx.openai.azure.com/
    public const string AzureOpenAiApiKey = "<<INSERT YOUR FOUNDRY AI API KEY>>";
    public const string AzureOpenAiDeployment = "gpt-4o-mini";
}
