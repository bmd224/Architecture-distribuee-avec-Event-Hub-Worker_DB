using MVC.Models;
using Microsoft.Extensions.Options;
using Azure.Messaging.ServiceBus;
using System.Text.Json;

namespace MVC.Business
{
    public class ServiceBusController
    {
        //Configuration récupérée depuis Azure App Configuration (avec IOptions)
        private ApplicationConfiguration _applicationConfiguration { get; }
        // Service Bus
        private ServiceBusClientOptions _serviceBusClientOptions { get; }

        public ServiceBusController(IOptionsSnapshot<ApplicationConfiguration> options)
        {
            _applicationConfiguration = options.Value;

            _serviceBusClientOptions = new ServiceBusClientOptions
            {
                RetryOptions = new ServiceBusRetryOptions
                {
                    Delay = TimeSpan.FromSeconds(10), //delai entre les tentatives
                    MaxDelay = TimeSpan.FromSeconds(60), // delai maximum entre les tentatives
                    Mode = ServiceBusRetryMode.Exponential,
                    MaxRetries = 6,
                },
                TransportType = ServiceBusTransportType.AmqpWebSockets,
                ConnectionIdleTimeout = TimeSpan.FromMinutes(10)
            };
        }

        //Cette méthode utilitaire pour envoyer un message
        private async Task SendMessageAsync(string queueName, ServiceBusMessage message, int Defer = 0)
        {
            await using ServiceBusClient serviceBusClient = new ServiceBusClient(_applicationConfiguration.ServiceBusConnectionString, _serviceBusClientOptions);
            ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);

            if (Defer != 0)
            {
                //Si il yaa délai  
                DateTimeOffset scheduleTime = DateTimeOffset.Now.AddMinutes(5);
                await serviceBusSender.ScheduleMessageAsync(message, scheduleTime);
            }
            else
            {
                //Sinon, envoi immédiat
                await serviceBusSender.SendMessageAsync(message);
            }
        }

        // Envoi d’un message dans la file de resize d’image
        public async Task SendImageToResize(Guid imageName, Guid Id)
        {
            Console.WriteLine("Envoi d'un message pour ImageResize : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new Tuple<Guid, Guid>(imageName, Id)));
            await SendMessageAsync(_applicationConfiguration.SB_resizeQueueName, message);
        }

        //Envoi d’un message texte pour validation de contenu via Azure Content Safety
        public async Task SendContentTextToValidation(string text, Guid commentId, Guid postId)
        {
            Console.WriteLine("Envoi d'un message pour Text Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(new ContentTypeValidation(ContentType.Text, text, commentId, postId)));
            await SendMessageAsync(_applicationConfiguration.SB_contentQueueName, message, 0);
        }

        //Envoi d’une image pour validation de contenu  (5 minutes)
        public async Task SendContentImageToValidation(Guid imageName, Guid commentId, Guid postId)
        {
            Console.WriteLine("Envoi d'un message pour Image Content Validation : " + DateTime.Now.ToString());
            ServiceBusMessage message = new ServiceBusMessage(JsonSerializer.Serialize(
                new ContentTypeValidation(ContentType.Image, imageName.ToString(), commentId, postId)));

            await SendMessageAsync(_applicationConfiguration.SB_contentQueueName, message, 5);
        }
    }
}
