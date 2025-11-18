using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MVC.Models;

namespace MVC.Business
{
    public class EventHubController
    {
        // Logsger pour le suivi des événements et erreurs
        private readonly ILogger<EventHubController> _logger;
        private readonly EventHubProducerClient _eventHubProducerClient;

        // Constructeur avec injection du logger et initialisation du Event Hub
        public EventHubController(ILogger<EventHubController> logger, string eventHubConnectionString)
        {
            _logger = logger;

            // Configuration des options de retry pour le client Event Hub
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

            // Nom de l'Event Hub
            string eventHubName = "cours4_events";
            _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName, producerOptions);
        }
        // Envoie un événement sérialisé vers Event Hub
        public async Task SendEventAsync(Event message)
        {
            try
            {
                _logger.LogInformation("{Time} : Envoi d’un évènement : {Action} {MediaType} {PostId}", DateTime.Now, message.Action, message.MediaType, message.PostId);

                // Création d’un batch pour l’envoi
                using EventDataBatch batch = await _eventHubProducerClient.CreateBatchAsync();

                // Sérialisation de l’événement en JSON
                var eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)))
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString()
                };

                // Vérifie si l'événement peut être ajouté au batch
                if (!batch.TryAdd(eventData))
                {
                    _logger.LogWarning("L’évènement est trop volumineux pour être envoyé dans un batch.");
                    return;
                }

                // Envoi du batch vers Event Hub
                await _eventHubProducerClient.SendAsync(batch);
                _logger.LogInformation("Évènement envoyé avec succès vers Event Hub.");
            }
            catch (Exception ex)
            {
                // Log en cas d’erreur
                _logger.LogError(ex, "Erreur lors de l’envoi de l’évènement vers Event Hub.");
            }
        }
    }
}
