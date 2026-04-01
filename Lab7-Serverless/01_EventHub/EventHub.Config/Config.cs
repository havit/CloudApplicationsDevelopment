namespace EventHub.Config;

/// <summary>
/// Centrální konfigurace pro všechny Event Hub projekty.
/// Vyplňte hodnoty zde – změna se projeví ve všech projektech.
/// </summary>
public static class Config
{
    // ===== EVENT HUB =====
    // Azure Portal → Event Hub Namespace → Shared access policies → connection string
    public const string EventHubConnectionString = "<<INSERT YOUR CONNECTION STRING>>";
    public const string EventHubName = "<<INSERT YOUR HUB NAME>>";

    // ===== AZURE OPENAI (Foundry) =====
    // ai.azure.com → váš projekt → Deployments → vyberte model → zkopírujte Endpoint a Key
    public const string AzureOpenAiEndpoint = "<<INSERT YOUR AZURE OPENAI ENDPOINT>>"; // https://xxx.openai.azure.com/
    public const string AzureOpenAiApiKey = "<<INSERT YOUR AZURE OPENAI API KEY>>";
    public const string AzureOpenAiDeployment = "gpt-4o-mini"; // název deploymentu ve Foundry
}
