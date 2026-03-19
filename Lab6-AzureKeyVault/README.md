# LAB6 - Azure KeyVault & Managed Identity

## Managed Identity

* Na App Service v záložce Identity zapneme System assigned identitu.

### Použití Managed Identity pro přístup k Azure Storage Blob (v ASP.NET Core aplikaci).
* V blob storage, na containeru, přidáme identitě aplikace oprávnění Storage Blob Data Contributor.
* Upravíme aplikaci tak, aby pro přístup k Blob Storage použila svoji identitu (potřebujeme nuget balíček Azure.Identity) a to tak, že použijeme jiný konstruktor třídy BlobContainerClient.

```
string containerEndpoint = $"https://{AzureStorageName}.blob.core.windows.net/{AzureStorageContainerName}";
return new BlobContainerClient(new Uri(containerEndpoint), new Azure.Identity.DefaultAzureCredential());
```

### Použití Managed Identity pro čtení konfigurace z Azure Key Vault (v ASP.NET Core aplikaci).
* Založíme KeyVault (na záložce Access configuration použijeme Azure role-based access control, jde o výchozí hodnotu).
* Identitě aplikace přidáme oprávnění Key Vault Secrets User.
* Použijeme nuget balíček Azure.Extensions.AspNetCore.Configuration.Secrets.
* Použijeme provider konfigurace.

```
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(new Uri("https://<my-key-vault>.vault.azure.net"), new Azure.Identity.DefaultAzureCredential());
}
```

## Azure Key Vault
* Identitě přidáme oprávnění čtení Secrets (pro Vault access policy).
* Identitě přidáme oprávnění Key Vault Secrets User (pro RBAC).
* Identitě přidáme oprávnění Key Vault Crypto User (pro RBAC).
* Vyzkoušíme v konzolovce práci se Secrets a Keys.

## Troubleshooting

Pokud se nám nedaří založit v Azure Key Vault secret nebo klíč, může to být tím, že sami nemáme dostatečná oprávnění.