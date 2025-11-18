using Azure.Security.KeyVault.Secrets;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MVC.Business;
using Worker_Content.Business;

namespace Worker_Content
{
    public class Worker_Content
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Worker>();

            // Credentials Azure
            var credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeEnvironmentCredential = false
            });

            // App Configuration Endpoint
            string appConfigEndpoint = builder.Configuration["Endpoints:AppConfiguration"]!;

            // Création du client App Configuration
            var appConfigClient = new ConfigurationClient(new Uri(appConfigEndpoint), credentials);

            // Lecture des paramètres
            var keyVaultEndpoint = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault").Value.Value;
            var contentSafetyEndpoint = appConfigClient.GetConfigurationSetting("Endpoints:ContentSafety").Value.Value;
            var blobContainer1 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:UnvalidatedBlob").Value.Value;
            var blobContainer2 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:ValidatedBlob").Value.Value;

            // Création du client Key Vault
            var keyVaultClient = new SecretClient(new Uri(keyVaultEndpoint), credentials);

            // Lecture des secrets depuis Key Vault
            var blobKey = keyVaultClient.GetSecret("ConnectionStringBlob").Value;
            var sbKey = keyVaultClient.GetSecret("ConnectionStringSB").Value;
            var cosmosKey = keyVaultClient.GetSecret("ConnectionStringCosmosDB").Value;
            var contentSafetyKey = keyVaultClient.GetSecret("ConnectionStringContentSafety").Value;
            var appInsightKey = keyVaultClient.GetSecret("ConnectionStringApplicationInsight").Value;
            var eventHubKey = keyVaultClient.GetSecret("ConnectionStringEventHub").Value;

            // Injection des options
            builder.Services.Configure<WorkerOptions>(options =>
            {
                options.BlobStorageKey = blobKey.Value;
                options.BlobContainer1 = blobContainer1;
                options.BlobContainer2 = blobContainer2;
                options.ServiceBusKey = sbKey.Value;
                options.CosmosDbKey = cosmosKey.Value;
                options.ContentSafetyKey = contentSafetyKey.Value;
                options.ContentSafetyEndpoint = contentSafetyEndpoint;
            });

            // Event Hub Service
            builder.Services.AddSingleton<EventHubService>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<EventHubService>>();
                var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "EventHub:ConnectionString", eventHubKey.Value },
                    { "EventHub:HubName", "cours4_events" }
                }).Build();
                return new EventHubService(logger, config);
            });

            // Application Insights
            builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Worker_Content", Environment.GetEnvironmentVariable("HOSTNAME")!));
            builder.Services.AddLogging(logging =>
            {
                logging.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) =>
                        config.ConnectionString = appInsightKey.Value,
                    configureApplicationInsightsLoggerOptions: (options) => { });
                logging.AddFilter<ApplicationInsightsLoggerProvider>("Worker_Content", LogLevel.Trace);
            });

            builder.Services.AddApplicationInsightsTelemetryWorkerService();
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
                o.ConnectionString = appInsightKey.Value;
            });

            var host = builder.Build();
            host.Run();
        }
    }
    // Cette classe pour les secrets et paramètres configurés dans Azure
    public class WorkerOptions
    {
        public required string BlobStorageKey { get; set; }
        public required string BlobContainer1 { get; set; }
        public required string BlobContainer2 { get; set; }
        public required string ServiceBusKey { get; set; }
        public required string CosmosDbKey { get; set; }
        public required string ContentSafetyKey { get; set; }
        public required string ContentSafetyEndpoint { get; set; }
    }
}

