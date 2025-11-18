using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Processor;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Worker_DB.Models;
using Worker_DB.Services;
using Microsoft.Extensions.Options;

namespace Worker_DB
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private EventProcessorClient _eventProcessorClient;
        private ConcurrentQueue<EventData> _messageQueue;
        private readonly WorkerOptions _options;

        private Microsoft.Azure.Cosmos.Container _containerPosts;
        private CosmosClient _cosmosClient;
        private CommentRepository _commentRepository;

        private SemaphoreSlim _semaphore;
        private const int ConcurentJobLimit = 1;

        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options)
        {
            
            _logger = logger;
            _options = options.Value;

            string eventHubConnection = _options.EventHubConnectionString;
            string blobConnection = _options.BlobStorageKey;
            string cosmosDbConnection = _options.CosmosDbKey;

            BlobContainerClient blobContainerClient = new BlobContainerClient(blobConnection, "synchro");
            _eventProcessorClient = new EventProcessorClient(blobContainerClient, "$Default", eventHubConnection);

            _messageQueue = new ConcurrentQueue<EventData>();
            _eventProcessorClient.ProcessEventAsync += MessageHandler;
            _eventProcessorClient.ProcessErrorAsync += ErrorHandler;

            _semaphore = new SemaphoreSlim(ConcurentJobLimit);

            _cosmosClient = new CosmosClient(cosmosDbConnection, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                EnableTcpConnectionEndpointRediscovery = true
            });

            _containerPosts = _cosmosClient.GetContainer("ApplicationDB", "Posts");
            var commentContainer = _cosmosClient.GetContainer("ApplicationDB", "Comments");
            _commentRepository = new CommentRepository(_cosmosClient, "ApplicationDB", "Comments");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _eventProcessorClient.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(1000, stoppingToken);
            }

            await _eventProcessorClient.StopProcessingAsync(stoppingToken);
        }

        private async Task MessageHandler(ProcessEventArgs args)
        {
            await _semaphore.WaitAsync();
            _messageQueue.Enqueue(args.Data);
            _ = ProcessMessagesAsync(args);
        }

        // Traitement des messages
        private async Task ProcessMessagesAsync(ProcessEventArgs args)
        {
            try
            {
                var messageBody = Encoding.UTF8.GetString(args.Data.Body.ToArray());
                var eventObj = JsonSerializer.Deserialize<Event>(messageBody);

                _logger.LogInformation("Événement reçu : Action = {0}, MediaType = {1}, PostId = {2}, Data = {3}, CommentId = {4}",
                    eventObj.Action, eventObj.MediaType, eventObj.PostId, eventObj.Data, eventObj.CommentId);

                // Ajout d'un nouveau post (image)
                if (eventObj.Action == EventAction.Submitted)
                {
                    if (eventObj.MediaType == MediaType.Image)
                    {
                        dynamic post = new
                        {
                            id = eventObj.PostId.ToString(),
                            PostId = eventObj.PostId,
                            Title = "From EventHub",
                            Category = 0,
                            User = "System",
                            Created = DateTime.UtcNow,
                            Like = 0,
                            Dislike = 0,
                            IsApproved = (bool?)null,
                            IsDeleted = false,
                            BlobImage = eventObj.Data,
                            Url = $"https://yomanunsixsixblobstore.blob.core.windows.net/unvalidated/{eventObj.Data}"
                        };

                        await _containerPosts.CreateItemAsync(post, new PartitionKey(eventObj.PostId.ToString()));
                    }
                    // Ajout d’un nouveau commentaire
                    else if (eventObj.MediaType == MediaType.Text)
                    {
                        var comment = new Comment
                        {
                            Id = eventObj.CommentId ?? Guid.NewGuid(),
                            PostId = eventObj.PostId,
                            Commentaire = eventObj.Data,
                            Created = DateTime.UtcNow,
                            Like = 0,
                            Dislike = 0,
                            IsApproved = null,
                            IsDeleted = false,
                            User = "System"
                        };

                        await _commentRepository.AddCommentAsync(comment);
                    }
                }
                // Validation d’un post (image)
                else if (eventObj.Action == EventAction.Validated && eventObj.MediaType == MediaType.Image)
                {
                    var patch = new[]
                    {
                PatchOperation.Replace("/IsApproved", true),
                PatchOperation.Replace("/Url", $"https://yomanunsixsixblobstore.blob.core.windows.net/validated/{eventObj.Data}")
            };

                    await _containerPosts.PatchItemAsync<dynamic>(
                        id: eventObj.PostId.ToString(),
                        partitionKey: new PartitionKey(eventObj.PostId.ToString()),
                        patchOperations: patch);
                }
                // Refus d’un post (image)
                else if (eventObj.Action == EventAction.Refused && eventObj.MediaType == MediaType.Image)
                {
                    var patch = new[]
                    {
                PatchOperation.Replace("/IsApproved", false)
            };

                    await _containerPosts.PatchItemAsync<dynamic>(
                        id: eventObj.PostId.ToString(),
                        partitionKey: new PartitionKey(eventObj.PostId.ToString()),
                        patchOperations: patch);
                }
                // Validation d’un commentaire
                else if (eventObj.Action == EventAction.Validated && eventObj.MediaType == MediaType.Text)
                {
                    var patch = new[]
                    {
                PatchOperation.Replace("/IsApproved", true)
            };

                    await _commentRepository.PatchCommentAsync(eventObj.CommentId!.Value, eventObj.PostId, patch);
                }
                // Refus d’un commentaire
                else if (eventObj.Action == EventAction.Refused && eventObj.MediaType == MediaType.Text)
                {
                    var patch = new[]
                    {
                PatchOperation.Replace("/IsApproved", false)
            };

                    await _commentRepository.PatchCommentAsync(eventObj.CommentId!.Value, eventObj.PostId, patch);
                }
                else if (eventObj.Action == EventAction.Deleted)
                {
                    if (eventObj.MediaType == MediaType.Image)
                    {
                        await _containerPosts.DeleteItemAsync<dynamic>(
                            id: eventObj.PostId.ToString(),
                            partitionKey: new PartitionKey(eventObj.PostId.ToString()));
                    }
                    else if (eventObj.MediaType == MediaType.Text)
                    {
                        await _cosmosClient.GetContainer("ApplicationDB", "Comments")
                            .DeleteItemAsync<dynamic>(
                                id: eventObj.CommentId.ToString(),
                                partitionKey: new PartitionKey(eventObj.PostId.ToString()));
                    }
                }
                // Marque l'événement comme traité
                await args.UpdateCheckpointAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors du traitement du message.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Gestion des erreurs Event Hub
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Erreur dans le EventProcessorClient.");
            return Task.CompletedTask;
        }
    }
}
