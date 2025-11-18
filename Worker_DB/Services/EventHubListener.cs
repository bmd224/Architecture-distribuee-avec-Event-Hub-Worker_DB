using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Consumer;
using System.Text;
using System.Threading.Tasks;

namespace Worker_DB.Services
{
    public class EventHubListener
    {
        private readonly string connectionString;
        private readonly string eventHubName;
        private readonly string consumerGroup;

        public EventHubListener(string connectionString, string eventHubName)
        {
            this.connectionString = connectionString;
            this.eventHubName = eventHubName;
            this.consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken)
        {
            await foreach (PartitionEvent partitionEvent in new EventHubConsumerClient(
                consumerGroup, connectionString, eventHubName).ReadEventsAsync(cancellationToken))
            {
                string content = Encoding.UTF8.GetString(partitionEvent.Data.Body.ToArray());

                Console.WriteLine($"[EventHub] Message reçu : {content}");
            }
        }
    }
}
