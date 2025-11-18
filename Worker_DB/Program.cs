using Azure.Security.KeyVault.Secrets;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Worker_DB;
using Worker_DB.Services;

var builder = Host.CreateApplicationBuilder(args);

// Enregistrement du service Worker
builder.Services.AddHostedService<Worker>();

// Configuration des credentials Azure à partir des variables d'environnement
var credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeEnvironmentCredential = false,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeSharedTokenCacheCredential = true
});

// Lecture du endpoint App Configuration depuis appsettings.json ou Azure App Config
string appConfigEndpoint = builder.Configuration["Endpoints:AppConfiguration"]!;

// Création du client App Configuration
var appConfigClient = new ConfigurationClient(new Uri(appConfigEndpoint), credentials);

// Récupération de l'URL de Key Vault
var keyVaultSetting = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault").Value.Value;

// Création du Key Vault
var keyVaultClient = new SecretClient(new Uri(keyVaultSetting), credentials);

// Récupération des secrets
KeyVaultSecret eventHubSecret = keyVaultClient.GetSecret("ConnectionStringEventHub");
KeyVaultSecret cosmosSecret = keyVaultClient.GetSecret("ConnectionStringCosmosDB");
KeyVaultSecret blobSecret = keyVaultClient.GetSecret("ConnectionStringBlob");

// Injection des secrets dans la configuration du worker
builder.Services.Configure<WorkerOptions>(options =>
{
    options.EventHubConnectionString = eventHubSecret.Value;
    options.CosmosDbKey = cosmosSecret.Value;
    options.BlobStorageKey = blobSecret.Value;
});


var host = builder.Build();
host.Run();

// Cette classe pour les secrets et paramètres configurés dans Azure
public class WorkerOptions
{
    public required string EventHubConnectionString { get; set; }
    public required string CosmosDbKey { get; set; }
    public required string BlobStorageKey { get; set; }
}
