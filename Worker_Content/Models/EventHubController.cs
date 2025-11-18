using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MVC.Models;
using Worker_Content.Models;

namespace MVC.Business
{
    public class EventHubController
    {
        private readonly ILogger<EventHubController> _logger;
        // Client producer pour envoyer des événements vers Event Hub
        private readonly EventHubProducerClient _eventHubProducerClient;

        // Constructeur avec injection du logger et de la chaîne de connexion Event Hub
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
            //Nom de l'Event Hub
            string eventHubName = "cours4_events";
            // Création du client Event Hub Producer
            _eventHubProducerClient = new EventHubProducerClient(eventHubConnectionString, eventHubName, producerOptions);
        }

        // Envoi asynchrone d’un événement vers Event Hub
        public async Task SendEventAsync(Event message)
        {
            try
            {
                _logger.LogInformation("{Time} : Envoi d’un évènement : {Action} {MediaType} {PostId}", DateTime.UtcNow, message.Action, message.MediaType, message.PostId);

                using EventDataBatch batch = await _eventHubProducerClient.CreateBatchAsync();

                // Sérialisation de l'objet Event en JSON, puis en bytes
                var eventData = new EventData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message)))
                {
                    ContentType = "application/json",
                    MessageId = Guid.NewGuid().ToString() // ID unique pour chaque message
                };

                // Vérification si le batch peut contenir l’événement
                if (!batch.TryAdd(eventData))
                {
                    _logger.LogWarning("L’évènement est trop volumineux pour être envoyé dans un batch.");
                    return;
                }

                // Envoi du batch vers Event Hub
                await _eventHubProducerClient.SendAsync(batch);
                //logs encore
                _logger.LogInformation("Évènement envoyé avec succès vers Event Hub.");
            }
            catch (Exception ex)
            {
                // Log d'erreur en cas d’échec d’envoi
                _logger.LogError(ex, "Erreur lors de l’envoi de l’évènement vers Event Hub.");
            }
        }
    }
}
