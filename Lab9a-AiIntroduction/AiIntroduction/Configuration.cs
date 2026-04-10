using System;
using System.Collections.Generic;
using System.Text;

namespace AiIntroduction
{
    // TODO: Doplnit konfiguraci hodnotami, které nám vygeneruje Microsoft Foundry po založení projektu a deploymentu modelu

    /// <summary>
    /// Konfiguraci, kterou je potřeba nastavit potom, co si založíme Microsoft Foundry projekt a v něm Discover model (Discover / Model / Deploy) 
    /// </summary>
    public static class Configuration
    {
        public const string ApiKey = ""; // po vytvoření projektu je nám vygenerován API key
        public const string Endpoint = "https://<resource_name>.openai.azure.com/openai/v1"; // název resource, který jsme si nastavili
        public const string DeploymentName = "<deployment_name>"; // název deploymentu, který jsme si nastavili při zakládání, např. "gpt-4o"
    }
}
