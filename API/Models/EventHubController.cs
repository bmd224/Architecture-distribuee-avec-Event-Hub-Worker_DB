using Azure.Messaging.EventHubs.Producer;
using Azure.Messaging.EventHubs;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace API.Models
{
    public class EventHubController
    {
        // Logger pour le suivi des événements et erreurs
        private readonly ILogger<EventHubController> _logger;
        //envoi d'événements vers Event Hub
        private readonly EventHubProducerClient _eventHubProducerClient;

        public EventHubController(ILogger<EventHubController> logger, string eventHubConnectionString)
        {
            _logger = logger;

            // Configuration du comportement en cas d'échec
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

            // Nom de l'event hub
            string eventHubName = "cours4_events";
            _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName, producerOptions);
        }

        public async Task SendEventAsync(Event message)
        {
            try
            {
                _logger.LogInformation("{Time} : Envoi d’un évènement : {Action} {MediaType} {PostId}", DateTime.UtcNow, message.Action, message.MediaType, message.PostId);

                // j'ai Cree un batch pour regrouper les messages
                using EventDataBatch batch = await _eventHubProducerClient.CreateBatchAsync();

                // Sérialisation de l’événement en JSON
                var eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)))
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                //on  vérifie si le batch peut contenir l’événement
                if (!batch.TryAdd(eventData))
                {
                    _logger.LogWarning("L’évènement est trop volumineux pour être envoyé dans un batch.");
                    return;
                }

                // Ensuite envoi du batch vers Event Hub
                await _eventHubProducerClient.SendAsync(batch);
                _logger.LogInformation("Évènement envoyé avec succès vers Event Hub.");
            }
            // Log d'erreur en cas d’échec d’envoi pour m'aider a deboguer
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l’envoi de l’évènement vers Event Hub.");
            }
        }
    }
}
