# LAB5 - Application Insights

[Application Insights for ASP.NET Core applications](https://docs.microsoft.com/en-us/azure/azure-monitor/app/asp-net-core)

* Založíme Application Insights
* Do aplikace přidáme nuget balíček [Microsoft.ApplicationInsights.AspNetCore](https://www.nuget.org/packages/Microsoft.ApplicationInsights.AspNetCore).
* Do services zaregistrujeme služby application insights (services.AddApplicationInsightsTelemetry()).
* Do appsettings.json nastavíme InstrumentationKey na hodnotu ze založené služby.

