using Azure.Core;


using Microsoft.Extensions.Options;

// Service Bus
using Azure.Messaging.ServiceBus;
using System.Collections.Concurrent;

// Blob
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;

// Images
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

// Json
using System.Text.Json;

// Event Hub
using MVC.Business;
using Worker_Image.Models;
using Azure.Storage.Blobs.Models;

namespace Worker_Image
{

    public class Worker : BackgroundService
    {
        // Déclarations des dépendances
        private readonly ILogger<Worker> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly ServiceBusProcessor _processor;
        private readonly ConcurrentQueue<ServiceBusReceivedMessage> _messageQueue;
        private readonly WorkerOptions _options;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly EventHubController _eventHubController;
        private readonly SemaphoreSlim _semaphore;

        // Limite de tâches simultanées et délai de traitement
        private const int ConcurentJobLimit = 5;
        private const int ProcessingDelayMS = 30000;

        public Worker(ILogger<Worker> logger, IOptions<WorkerOptions> options, EventHubController eventHubController)
        {
            _logger = logger;
            _options = options.Value;
            _eventHubController = eventHubController;

            // Configuration du client Blob
            BlobClientOptions blobClientOptions = new BlobClientOptions
            {
                Retry = {
                    Delay = TimeSpan.FromSeconds(2),
                    MaxRetries = 5,
                    Mode = RetryMode.Exponential,
                    MaxDelay = TimeSpan.FromSeconds(10)
                },
            };

            // Initialisation de la file de messages
            _blobServiceClient = new BlobServiceClient(_options.BlobStorageKey, blobClientOptions);

            // Nom de la queue
            _messageQueue = new ConcurrentQueue<ServiceBusReceivedMessage>();
            string queueName = "imageresizemessage";

            // Configuration du client Service Bus
            ServiceBusClientOptions clientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(10),
                    MaxDelay = TimeSpan.FromSeconds(60),
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 6,
                },
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                ConnectionIdleTimeout = TimeSpan.FromMinutes(10)
            };
            //Client et du processor
            _serviceBusClient = new ServiceBusClient(_options.ServiceBusKey, clientOptions);
            _processor = _serviceBusClient.CreateProcessor(queueName, new ServiceBusProcessorOptions
            {
                MaxConcurrentCalls = 5,
                AutoCompleteMessages = false
            });

            // Liaison des événements
            _processor.ProcessMessageAsync += MessageHandler;
            _processor.ProcessErrorAsync += ErrorHandler;

            _semaphore = new SemaphoreSlim(ConcurentJobLimit);
        }

        // Gère la réception d’un message
        private async Task MessageHandler(ProcessMessageEventArgs args)
        {
            await _semaphore.WaitAsync();
            _messageQueue.Enqueue(args.Message);
            _ = ProcessMessagesAsync(args);
        }

        // Traitement d’un message
        private async Task ProcessMessagesAsync(ProcessMessageEventArgs args)
        {
            try
            {
                var message = JsonSerializer.Deserialize<Tuple<Guid, Guid>>(args.Message.Body.ToString());
                if (message == null)
                {
                    throw new InvalidOperationException("Message deserialization failed.");
                }

                _logger.LogInformation("Processing message: {MessageId}, PostId : {PostId}, Image : {ImageId}", args.Message.MessageId, message.Item2, message.Item1);

                using MemoryStream ms = new MemoryStream();

                try
                {
                    await ProcessImageAsync(message.Item1, ms);

                    // Envoi d'un EventHub Event
                    await _eventHubController.SendEventAsync(new Event
                    {
                        MediaType = MediaType.Image,
                        Action = EventAction.Processed,
                        PostId = message.Item2,
                        CommentId = null,
                        Data = message.Item1.ToString()
                    });

                    await args.CompleteMessageAsync(args.Message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing image for message: {MessageId}", args.Message.MessageId);
                    await HandleMessageProcessingErrorAsync(args);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message: {MessageId}", args.Message.MessageId);
                await HandleMessageErrorAsync(args);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        // Traitement de l’image
        private async Task ProcessImageAsync(Guid imageId, MemoryStream ms)
        {
            bool Container1 = await _blobServiceClient.GetBlobContainerClient(_options.BlobContainer1).GetBlobClient(imageId.ToString()).ExistsAsync();

            var blob = _blobServiceClient.GetBlobContainerClient(Container1 ? _options.BlobContainer1 : _options.BlobContainer2).GetBlockBlobClient(imageId.ToString());
            await blob.DownloadToAsync(ms);
            ms.Position = 0;

            using (var image = Image.Load(ms))
            {
                image.Mutate(c => c.Resize(500, 0));
                ms.Position = 0;
                await image.SaveAsPngAsync(ms);
                ms.Position = 0;
            }

            await Task.Delay(ProcessingDelayMS);
            await blob.UploadAsync(ms, new BlobUploadOptions
            {
                Conditions = null 
            });
            
        }

        // Gère les erreurs de traitement d’image
        private async Task HandleMessageProcessingErrorAsync(ProcessMessageEventArgs args)
        {
            if (args.Message.DeliveryCount > 5)
            {
                await args.DeadLetterMessageAsync(args.Message, "Image Processing Error", "Exceeded maximum retries");
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);
            }
        }

        // Gère les erreurs de message
        private async Task HandleMessageErrorAsync(ProcessMessageEventArgs args)
        {
            if (args.Message.DeliveryCount > 5)
            {
                await args.DeadLetterMessageAsync(args.Message, "Processing Error", "Exceeded maximum retries");
            }
            else
            {
                await args.AbandonMessageAsync(args.Message);
            }
        }

        // Gère les erreurs
        private Task ErrorHandler(ProcessErrorEventArgs args)
        {
            _logger.LogError(args.Exception, "Error processing messages.");
            return Task.CompletedTask;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _processor.StartProcessingAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                await Task.Delay(5000, stoppingToken);
            }

            await _processor.StopProcessingAsync(stoppingToken);
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await _processor.CloseAsync();
            await base.StopAsync(stoppingToken);
        }
    }
}
