using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Worker_Image.Models;

namespace MVC.Business
{
    public class EventHubController
    {
        private readonly ILogger<EventHubController> _logger;
        private readonly EventHubProducerClient _eventHubProducerClient;

        public EventHubController(ILogger<EventHubController> logger, string eventHubConnectionString)
        {
            _logger = logger;

            var producerOptions = new EventHubProducerClientOptions
            {
                RetryOptions = new EventHubsRetryOptions
                {
                    MaximumRetries = 5,
                    Delay = TimeSpan.FromSeconds(5),
                    MaximumDelay = TimeSpan.FromSeconds(50),
                    Mode = EventHubsRetryMode.Exponential
                }
            };

            string eventHubName = "cours4_events";
            _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName, producerOptions);
        }

        public async Task SendEventAsync(Event message)
        {
            try
            {
                _logger.LogInformation("{Time} : Envoi d’un évènement : {Action} {MediaType} {PostId}", DateTime.UtcNow, message.Action, message.MediaType, message.PostId);

                using EventDataBatch batch = await _eventHubProducerClient.CreateBatchAsync();

                var eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)))
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                if (!batch.TryAdd(eventData))
                {
                    _logger.LogWarning("L’évènement est trop volumineux pour être envoyé dans un batch.");
                    return;
                }

                await _eventHubProducerClient.SendAsync(batch);
                _logger.LogInformation("Évènement envoyé avec succès vers Event Hub.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l’envoi de l’évènement vers Event Hub.");
            }
        }
    }
}
