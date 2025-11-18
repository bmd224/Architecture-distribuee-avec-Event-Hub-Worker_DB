using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using MVC.Models;
using Worker_Content.Models;

namespace Worker_Content.Business
{
    public class EventHubService
    {
        private readonly ILogger<EventHubService> _logger;
        // Client EventHub pour produire des événements
        private readonly EventHubProducerClient _producer;

        // Constructeur
        public EventHubService(ILogger<EventHubService> logger, IConfiguration configuration)
        {
            _logger = logger;

            // Récupération de la chaîne de connexion et du nom de l’Event Hub
            var connectionString = configuration["EventHub:ConnectionString"]!;
            var hubName = configuration["EventHub:HubName"]!;

            //Client producteur est instancie ici
            _producer = new EventHubProducerClient(connectionString, hubName);
        }

        // Cette méthode pour envoyer un événement vers Event Hub
        public async Task SendEventAsync(Event evt)
        {
            try
            {
                // Création d’un batch d’événements (permet d’envoyer plusieurs messages ensemble)
                using EventDataBatch batch = await _producer.CreateBatchAsync();

                // Sérialisation de l’événement en JSON
                var payload = JsonSerializer.Serialize(evt);
                var data = new EventData(Encoding.UTF8.GetBytes(payload));

                // Vérification que l’événement peut être ajouté au batch
                if (!batch.TryAdd(data))
                {
                    _logger.LogWarning("L'évènement est trop volumineux pour être ajouté au batch.");
                    return;
                }

                // Envoi du batch vers Event Hub
                await _producer.SendAsync(batch);
                _logger.LogInformation("Event envoyé : {Action} - {Type} - {Id}", evt.Action, evt.MediaType, evt.CommentId ?? evt.PostId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de l'évènement.");
            }
        }


        // Méthode utilitaire pour construire un événement et l’envoyer
        public async Task SendValidationEventAsync(Guid? commentId, Guid postId, string data, bool isApproved)
        {
            // Vérification des paramètres Validé ou Refusé; Image ou Texte
            var action = isApproved ? EventAction.Validated : EventAction.Refused;
            var mediaType = commentId.HasValue ? MediaType.Text : MediaType.Image;

            // Construction de l’événement
            var evt = new Event
            {
                MediaType = mediaType,
                Action = action,
                PostId = postId,
                CommentId = commentId,
                Data = data
            };
            //Envoie de l’événement
            await SendEventAsync(evt);
        }
    }
}
