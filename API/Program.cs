using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.FeatureManagement;
using System.Reflection;

// Business
using MVC.Models;
using MVC.Data;
using MVC.Business;

// Monitoring
using Microsoft.AspNetCore.Http.HttpResults;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using API.Models;

var builder = WebApplication.CreateBuilder(args);

// OpenAPI
builder.Services.AddOpenApi();

// Configuration AppConfig
string AppConfigEndPoint = builder.Configuration.GetValue<string>("Endpoints:AppConfiguration")!;

//configuration du credential pour Azure
DefaultAzureCredential defaultAzureCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
{
    ExcludeSharedTokenCacheCredential = true,
    ExcludeVisualStudioCredential = true,
    ExcludeVisualStudioCodeCredential = true,
    ExcludeEnvironmentCredential = false
});

//Intégration de App Configuration et Key Vault
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(new Uri(AppConfigEndPoint), defaultAzureCredential);

    options.ConfigureKeyVault(kv =>
    {
        kv.SetCredential(defaultAzureCredential);
    });
});

builder.Services.AddAzureAppConfiguration();
builder.Services.AddFeatureManagement();
builder.Services.Configure<ApplicationConfiguration>(builder.Configuration.GetSection("ApplicationConfiguration"));

//Application Insights via OpenTelemetry
builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("ApplicationInsight");
    options.EnableLiveMetrics = true;
});

// configuration de la Bd
switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        builder.Services.AddDbContext<ApplicationDbContextSQL>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPISQL>();
        break;
    case "NoSQL":
        builder.Services.AddDbContext<ApplicationDbContextNoSQL>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPINoSQL>();
        break;
    case "InMemory":
        builder.Services.AddDbContext<ApplicationDbContextInMemory>();
        builder.Services.AddScoped<IRepositoryAPI, EFRepositoryAPIInMemory>();
        break;
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<FileUploadOperationFilter>(); // upload d'un fichier
    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")); // Ajout de commentaires en XML
});

// Injections des services blob et service bus
builder.Services.AddScoped<BlobController>();
builder.Services.AddScoped<ServiceBusController>();

//Injection du Event Hub avec récupération de la connectionstring
builder.Services.AddSingleton<EventHubController>(provider =>
{
    var logger = provider.GetRequiredService<ILogger<EventHubController>>();
    var config = provider.GetRequiredService<IConfiguration>();

    // Lecture depuis App Configuration + Key Vault
    var appConfigSection = config.GetSection("ApplicationConfiguration");
    var eventHubConnection = appConfigSection["EventHubConnectionString"];

    if (string.IsNullOrEmpty(eventHubConnection))
    {
        throw new InvalidOperationException("La chaîne de connexion Event Hub est manquante dans App Configuration.");
    }

    return new EventHubController(logger, eventHubConnection);
});


var app = builder.Build();

switch (builder.Configuration.GetValue<string>("DatabaseConfiguration"))
{
    case "SQL":
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContextSQL>();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.Migrate();
        }
        break;
    case "NoSQL":
        using (var scope = app.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContextNoSQL>();
        }
        break;
}

app.MapOpenApi();
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();

// API POSTS
app.MapGet("/Posts/", async (IRepositoryAPI repo) => await repo.GetAPIPostsIndex());
app.MapGet("/Posts/{id}", async (IRepositoryAPI repo, Guid id) => await repo.GetAPIPost(id));

app.MapPost("/Posts/Add", async (HttpRequest request, IFormFile image, BlobController blob, EventHubController eventHub, ServiceBusController serviceBus) =>
{
    Guid postId = Guid.NewGuid();
    var dto = new PostCreateDTO(request.Form["Title"]!, request.Form["Category"]!, request.Form["User"]!, image);

    Guid blobId = Guid.NewGuid();
    string url = await blob.PushImageToBlob(image, blobId);

    var post = new Post
    {
        Id = postId,
        Title = dto.Title,
        Category = dto.Category,
        User = dto.User,
        BlobImage = blobId,
        Url = url,
        Created = DateTime.UtcNow
    };

    await serviceBus.SendImageToResize(blobId, postId);
    await serviceBus.SendContentImageToValidation(blobId, Guid.Empty, postId);

    var eventMessage = new Event(post)
    {
        Data = blobId.ToString()
    };
    await eventHub.SendEventAsync(eventMessage);

    return Results.Ok("Post envoyé avec succès vers Event Hub");
}).DisableAntiforgery();

app.MapPost("/Posts/IncrementPostLike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementPostLike(id));
app.MapPost("/Posts/IncrementPostDislike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementPostDislike(id));

// API COMMENTS
app.MapGet("/Comments/{id}", async (IRepositoryAPI repo, Guid id) => await repo.GetAPIComment(id));

app.MapPost("/Comments/Add", async (IRepositoryAPI repo, CommentCreateDTO dto, EventHubController eventHub) =>
{
    var comment = new Comment
    {
        Id = Guid.NewGuid(),
        Commentaire = dto.Commentaire!,
        PostId = dto.PostId,
        User = dto.User!,
        Created = DateTime.UtcNow
    };

    var result = await repo.CreateAPIComment(dto);

    var eventMessage = new Event(comment)
    {
        Data = comment.Commentaire
    };
    await eventHub.SendEventAsync(eventMessage);

    return result;
});

app.MapPost("/Comments/IncrementCommentLike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementCommentLike(id));
app.MapPost("/Comments/IncrementCommentsDislike/{id}", async (IRepositoryAPI repo, Guid id) => await repo.APIIncrementCommentDislike(id));

app.Run();

public partial class Program { }