using Microsoft.EntityFrameworkCore;
using Azure.Identity;
using Microsoft.FeatureManagement;

// Project
using MVC.Data;
using MVC.Business;
using MVC.Models;

// Identity
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

// Monitoring
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.ApplicationInsights.DependencyCollector;

var builder = WebApplication.CreateBuilder(args);

// MVC services
builder.Services.AddControllersWithViews();

// Chargement d'Azure App Configuration
string appConfigEndpoint = builder.Configuration["Endpoints:AppConfiguration"]!;

// Credentials
DefaultAzureCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeSharedTokenCacheCredential = true,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeEnvironmentCredential = false
});

// Configuration d'Azure App Configuration avec Feature Flags + KeyVault
builder.Configuration.AddAzureAppConfiguration(config =>
{
    config.Connect(new Uri(appConfigEndpoint), credential)
          .Select("*")
          .UseFeatureFlags()
          .ConfigureRefresh(refresh =>
              refresh.Register("ApplicationConfiguration:Sentinel", refreshAll: true)
                     .SetRefreshInterval(TimeSpan.FromSeconds(30)));

    config.ConfigureKeyVault(kv => kv.SetCredential(credential));
});

// Ajout des services Azure App Configuration + Feature Management
builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();

builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

// Application Insights
builder.Services.AddSingleton<ITelemetryInitializer>(new CustomTelemetryInitializer("MVC", Environment.GetEnvironmentVariable("HOSTNAME")!));
// Configuration des logs vers Application Insights
builder.Services.AddLogging(logBuilder =>
{
    logBuilder.AddApplicationInsights(
        config => config.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsight")!,
        _ => { }
    );
    logBuilder.AddFilter<ApplicationInsightsLoggerProvider>("MVC", LogLevel.Trace);
});

// Ajout du service de télémétrie
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsight")!;
});

builder.Services.ConfigureTelemetryModule<DependencyTrackingTelemetryModule>((module, _) =>
{
    module.EnableSqlCommandTextInstrumentation = true;
});

// Choix du contexte de base de données
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        builder.Services.AddDbContext<ApplicationDbContextSQL>();
        builder.Services.AddScoped<IRepository, EFRepositorySQL>();
        break;

    case "NoSQL":
        builder.Services.AddDbContext<ApplicationDbContextNoSQL>();
        builder.Services.AddScoped<IRepository, EFRepositoryNoSQL>();
        break;

    case "InMemory":
        builder.Services.AddDbContext<ApplicationDbContextInMemory>();
        builder.Services.AddScoped<IRepository, EFRepositoryInMemory>();
        break;
}

// Dépendances applicatives
builder.Services.AddScoped<BlobController>();
builder.Services.AddScoped<ServiceBusController>();

// Injection du contrôleur EventHub avec récupération de la connection string
builder.Services.AddSingleton<EventHubController>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<EventHubController>>();
    var config = provider.GetRequiredService<IConfiguration>();

    // Récupération depuis App Configuration + Key Vault
    var appConfigSection = config.GetSection("ApplicationConfiguration");
    var eventHubConnection = appConfigSection["EventHubConnectionString"];

    if (string.IsNullOrEmpty(eventHubConnection))
    {
        throw new InvalidOperationException("La chaîne de connexion Event Hub est manquante dans App Configuration.");
    }

    return new EventHubController(logger, eventHubConnection);
});

// Authentification Microsoft
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllersWithViews(options =>
{
    var authPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(authPolicy));
});

builder.Services.AddRazorPages().AddMicrosoftIdentityUI();


builder.Services.AddHealthChecks();

var app = builder.Build();

// Initialisation des bases de données
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSQL>();
            context.Database.EnsureDeleted();
            context.Database.Migrate();
        }
        break;

    case "NoSQL":
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContextNoSQL>();
            await context.Database.EnsureCreatedAsync();
        }
        break;
}

// Middleware
app.UseAzureAppConfiguration();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHealthChecks("/healthz");
app.MapRazorPages();

app.Run();

public partial class Program { }