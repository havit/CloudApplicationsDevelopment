# LAB5 - Application Insights

[Tutorial: Build a .NET console app to manage data in Azure Cosmos DB SQL API account](https://docs.microsoft.com/en-us/azure/cosmos-db/sql/sql-api-get-started)

* Založíme Application Insights
* Do aplikace přidáme nuget balíček [Microsoft.ApplicationInsights.AspNetCore](https://www.nuget.org/packages/Microsoft.ApplicationInsights.AspNetCore).
* Do services zaregistrujeme služby application insights (services.AddApplicationInsights()).
* Do appsettings.json nastavíme InstrumentationKey na hodnotu ze založené služby.

