using Azure.Security.KeyVault.Secrets;
using Azure.Data.AppConfiguration;
using Azure.Identity;
using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using MVC.Business;

namespace Worker_Image
{
    public class Worker_Image
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            builder.Services.AddHostedService<Worker>();

            //Pour le Azure.Data.AppConfiguration
            string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

            // credentials
            DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                ExcludeSharedTokenCacheCredential = true,
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                ExcludeEnvironmentCredential = false
            });

            // Création du  App Config
            ConfigurationClient appConfigClient = new ConfigurationClient(new Uri(AppConfigEndPoint), defaultAzureCredential);
            ConfigurationSetting container1 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:UnvalidatedBlob");
            ConfigurationSetting container2 = appConfigClient.GetConfigurationSetting("ApplicationConfiguration:ValidatedBlob");

            // Création du  Key Vault
            ConfigurationSetting endpointKeyVault = appConfigClient.GetConfigurationSetting("Endpoints:KeyVault");
            SecretClient keyVaultClient = new SecretClient(new Uri(endpointKeyVault.Value), defaultAzureCredential);

            KeyVaultSecret blobKeyVault = keyVaultClient.GetSecret("ConnectionStringBlob");
            KeyVaultSecret servicebusKeyVault = keyVaultClient.GetSecret("ConnectionStringSB");
            KeyVaultSecret applicationinsightKeyVault = keyVaultClient.GetSecret("ConnectionStringApplicationInsight");
            KeyVaultSecret eventHubKeyVault = keyVaultClient.GetSecret("ConnectionStringEventHub");


            // Ajout de secrets a la configuration du worker
            builder.Services.Configure<WorkerOptions>(options =>
            {
                options.BlobStorageKey = blobKeyVault.Value;
                options.BlobContainer1 = container1.Value;
                options.BlobContainer2 = container2.Value;
                options.ServiceBusKey = servicebusKeyVault.Value;
                options.EventHubConnectionString = eventHubKeyVault.Value;

            });

            builder.Services.AddSingleton<EventHubController>(provider =>
            {
                var logger = provider.GetRequiredService<ILogger<EventHubController>>();
                return new EventHubController(logger, eventHubKeyVault.Value);
            });


            // Application Insight trace/log/metrics
            // https://medium.com/@chuck.beasley/how-to-instrument-a-net-5537ea851763
            builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("Worker_Image", Environment.GetEnvironmentVariable("HOSTNAME")!));
            builder.Services.AddLogging(logging =>
            {
                logging.AddApplicationInsights(
                    configureTelemetryConfiguration: (config) =>
                    config.ConnectionString = applicationinsightKeyVault.Value,
                    configureApplicationInsightsLoggerOptions: (options) => { }
                    );
                logging.AddFilter<ApplicationInsightsLoggerProvider>("Worker_Image", LogLevel.Trace);

            });
            builder.Services.AddApplicationInsightsTelemetryWorkerService();
            builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, o) =>
            {
                module.EnableSqlCommandTextInstrumentation = true;
                o.ConnectionString = applicationinsightKeyVault.Value;
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
        public required string EventHubConnectionString { get; set; }
    }
}



