using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace EventBus.ServiceBus;

public interface IServiceBusPersisterConnection : IAsyncDisposable
{
    ServiceBusClient TopicClient { get; }
    ServiceBusAdministrationClient AdministrationClient { get; }
}