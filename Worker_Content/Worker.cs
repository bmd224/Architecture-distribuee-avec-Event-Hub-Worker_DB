using Azure.Core;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.AI.ContentSafety;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using MVC.Models;
using Azure;
using Worker_Content.Business;
using Worker_Content.Models;
using ContentType = MVC.Models.ContentType;

namespace Worker_Content
{
    public class Worker : BackgroundService
    {
        // Logger pour écrire des informations et erreurs dans les logs
        private readonly ILogger<Worker> _logger;

        // Service qui permet d’envoyer un événement à Event Hub après validation
        private readonly EventHubService _eventHubService;

        // Client et processeur pour consommer des messages depuis Azure Service Bus
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;

        // File de traitement en mémoire pour les messages à consommer
        private readonly ConcurrentQueue<ServiceBusReceivedMessage> _messageQueue;

        // Options chargées depuis App Configuration / Key Vault
        private readonly WorkerOptions _options;

        // pour interagir avec Blob Storage, Cosmos DB, et Content Safety
        private readonly BlobServiceClient _blobServiceClient;
        private readonly CosmosClient _cosmosClient;
        private readonly Container _containerPosts;
        private readonly Container _containerComments;
        private readonly ContentSafetyClient _contentSafetyClient;

        // Sémaphore pour contrôler la concurrence du traitement
        private readonly SemaphoreSlim _semaphore;

        // Limite du nombre de traitements concurrents (ici = 1 pour plus de contrôle)
        private const int ConcurentJobLimit = 1;

        // Constructeur : initialisation de tous les services nécessaires
        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options, EventHubService eventHubService)
        {
            _logger = logger;
            _eventHubService = eventHubService;
            _options = options.Value;

            // Initialisation des clients Azure
            _contentSafetyClient = new ContentSafetyClient(new Uri(_options.ContentSafetyEndpoint), new AzureKeyCredential(_options.ContentSafetyKey));
            _cosmosClient = new CosmosClient(_options.CosmosDbKey);
            _containerPosts = _cosmosClient.GetContainer("ApplicationDB", "Posts");
            _containerComments = _cosmosClient.GetContainer("ApplicationDB", "Comments");
            _blobServiceClient = new BlobServiceClient(_options.BlobStorageKey);

            // Configuration du Service Bus
            _messageQueue = new ConcurrentQueue<ServiceBusReceivedMessage>();
            _serviceBusClient = new ServiceBusClient(_options.ServiceBusKey);
            _processor = _serviceBusClient.CreateProcessor("contentsafetymessage", new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            _semaphore = new SemaphoreSlim(ConcurentJobLimit);
        }

        // Gestionnaire appelé à chaque message reçu du Service Bus
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            await _semaphore.WaitAsync(); // contrôle du nombre de traitements
            _messageQueue.Enqueue(args.Message); // ajout du message dans la queue
            _ = ProcessMessagesAsync(args); // lancement du traitement
        }

        // Traitement du message en fonction de son type (texte ou image)
        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var message = JsonSerializer.Deserialize<ContentTypeValidation>(args.Message.Body.ToString());
                if (message == null) throw new Exception("Message null");

                bool isSafe;

                if (message.ContentType == ContentType.Text)
                {
                    // Validation du texte
                    isSafe = await ProcessTextValidationAsync(message.Content);
                    await UpdateCommentDatabaseAsync(message.CommentId, message.PostId, isSafe, message.Content);
                    await _eventHubService.SendValidationEventAsync(message.CommentId, message.PostId, message.Content, isSafe);
                }
                else // Si le contenu est une image
                {
                    using MemoryStream ms = new();
                    isSafe = await ProcessImageValidationAsync(message.Content, ms);
                    var uri = await UploadImageAsync(message.Content, ms);
                    await UpdatePostDatabaseAsync(message.PostId, uri, isSafe);
                    await DeleteImageAsync(message.Content);
                    await _eventHubService.SendValidationEventAsync(null, message.PostId, message.Content, isSafe);
                }

                await args.CompleteMessageAsync(args.Message); // message marqué comme traité
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur de traitement");
                await args.AbandonMessageAsync(args.Message); // message non traité
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Mise à jour du commentaire dans Cosmos DB après validation
        private async Task UpdateCommentDatabaseAsync(Guid commentId, Guid postId, bool isSafe, string commentaire)
        {
            var response = await _containerComments.ReadItemAsync<dynamic>(commentId.ToString(), new PartitionKey(postId.ToString()));
            var item = response.Resource;
            item.IsApproved = isSafe;
            item.Commentaire = commentaire;
            await _containerComments.ReplaceItemAsync(item, commentId.ToString(), new PartitionKey(postId.ToString()));
        }

        // Mise à jour du post dans Cosmos DB
        private async Task UpdatePostDatabaseAsync(Guid postId, Uri blobUri, bool isApproved)
        {
            try
            {
                var patchOps = new[]
                {
                    PatchOperation.Replace("/IsApproved", isApproved),
                    PatchOperation.Replace("/Url", blobUri.ToString())
                };

                await _containerPosts.PatchItemAsync<Post>(
                    id: postId.ToString(),
                    partitionKey: new PartitionKey(postId.ToString()),
                    patchOperations: patchOps);

                _logger.LogInformation($"Post {postId} mis à jour. Approved: {isApproved}");
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Si l'élément n'est pas encore dispo 
                _logger.LogWarning($"Post {postId} pas encore présent dans Cosmos. Nouvelle tentative dans 3s...");
                await Task.Delay(3000); // Attente
                await UpdatePostDatabaseAsync(postId, blobUri, isApproved); // Retry
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Échec mise à jour Post {postId}");
                throw;
            }
        }

        // Suppression de l’image du conteneur unvalidated
        private async Task DeleteImageAsync(string imageId)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(_options.BlobContainer1).GetBlockBlobClient(imageId);
            await blob.DeleteAsync();
        }

        // Envoi de l’image dans le conteneur validated
        private async Task<Uri> UploadImageAsync(string imageId, MemoryStream ms)
        {
            var blob2 = _blobServiceClient.GetBlobContainerClient(_options.BlobContainer2).GetBlockBlobClient(imageId);
            ms.Position = 0;
            await blob2.UploadAsync(ms);
            return blob2.Uri;
        }

        // Appel de l’API Content Safety pour analyser du texte
        private async Task<bool> ProcessTextValidationAsync(string text)
        {
            var response = await _contentSafetyClient.AnalyzeTextAsync(new AnalyzeTextOptions(text));
            return !response.Value.CategoriesAnalysis.Any(a => a.Severity > 0);
        }

        // Appel de l’API Content Safety pour analyser une image
        private async Task<bool> ProcessImageValidationAsync(string imageId, MemoryStream ms)
        {
            var blob = _blobServiceClient.GetBlobContainerClient(_options.BlobContainer1).GetBlockBlobClient(imageId);
            await blob.DownloadToAsync(ms);
            ms.Position = 0;
            var image = new ContentSafetyImageData(BinaryData.FromStream(ms));
            var response = await _contentSafetyClient.AnalyzeImageAsync(new AnalyzeImageOptions(image));

            return CheckImageSeverity(response.Value.CategoriesAnalysis);
        }

        // Méthode pour vérifier si l’image est considérée comme offensive
        private bool CheckImageSeverity(IReadOnlyList<ImageCategoriesAnalysis> analysis)
        {
            foreach (var category in analysis)
            {
                _logger.LogInformation($"[IMAGE] {category.Category} : Sévérité {category.Severity}");
                if (category.Severity >= 2) // 2  modéré pour l'instant
                    return false;
            }
            return true; // Aucun problème détecté
        }

        // En cas d’erreur du processeur Service Bus
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing message");
            return Task.CompletedTask;
        }

        // Démarrage du Worker et du traitement continu
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(5000, stoppingToken);
            }
            await _processor.StopProcessingAsync(stoppingToken);
        }

        //on ferme  le  Service Bus
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.CloseAsync();
            await base.StopAsync(stoppingToken);
        }
    }
}











