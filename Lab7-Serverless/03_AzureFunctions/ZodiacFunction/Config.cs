namespace ZodiacFunction;

/// <summary>
/// Konfigurace pro ZodiacFunction — Azure OpenAI (Foundry).
/// Vyplňte hodnoty zde před spuštěním.
/// </summary>
public static class Config
{
    // ===== AZURE OPENAI (Foundry) =====
    // ai.azure.com → váš projekt → Deployments → vyberte model → zkopírujte Endpoint a Key
	public const string AzureOpenAiEndpoint = "<<INSERT YOUR AZURE OPENAI ENDPOINT>>"; // https://xxx.openai.azure.com/
	public const string AzureOpenAiApiKey = "<<INSERT YOUR AZURE OPENAI API KEY>>";
    public const string AzureOpenAiDeployment = "gpt-4o-mini"; // název deploymentu ve Foundry
}
