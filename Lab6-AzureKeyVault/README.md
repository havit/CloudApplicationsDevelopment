# LAB6 - Azure KeyVault & Managed Identity

## Managed Identity

* Na App Service v záložce Identity zapneme System assigned identitu.
* V blob storage, na containeru, přidáme nově založené identitě oprávnění Storage Blob Data Contributor.
* Upravíme aplikaci tak, aby pro přístup k Blob Storage použila svoji identitu (potřebujeme nuget balíček Azure.Identity) a to tak, že použijeme jiný konstruktor třídy BlobContainerClient

```
string containerEndpoint = $"https://{AzureStorageName}.blob.core.windows.net/{AzureStorageContainerName}";
return new BlobContainerClient(new Uri(containerEndpoint), new Azure.Identity.DefaultAzureCredential());
```

## Azure Key Vault
* Založíme KeyVault (rozhodneme se pro způsob řízení oprávnění).

### Použití SDK
* Založíme aplikaci v Azure Active Directory (pokud nemůžeme použít vlastní identitu)
* Identitě aplikace přidáme oprávnění čtení Secrets (pro Vault access policy)
* Identitě aplikace přidáme oprávnění pro encrypt a decrypt s Keys (pro Vault access policy)
* Identitě aplikace přidáme oprávnění Key Vault Secrets User (pro RBAC)
* Identitě aplikace přidáme oprávnění Key Vault Crypto User (pro RBAC)
* Do konzolovky dopravíme TenantId, ClientId a ClientSecret (pokud nemůžeme použít vlastní identitu)
* Vyzkoušíme v konzolovce práci se Secrets a Keys

### Configuration provider pro ASP.NET Core aplikaci
* Identitě aplikace přidáme oprávnění pro listování a čtení Secrets (pro Vault access policy)
* Identitě aplikace přidáme oprávnění Key Vault Secrets User (pro RBAC)
* Použijeme nuget balíček Azure.Extensions.AspNetCore.Configuration.Secrets
* Použijeme provider konfigurace

```
if (builder.Environment.IsProduction())
{
	builder.Configuration.AddAzureKeyVault(new Uri("https://mffdemokeyvault.vault.azure.net"), new Azure.Identity.DefaultAzureCredential());
}
```
