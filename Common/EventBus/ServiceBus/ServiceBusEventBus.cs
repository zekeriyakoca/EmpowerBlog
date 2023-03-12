using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EventBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.ServiceBus
{
    public class ServiceBusEventBus : IEventBus
    {
        private readonly ILogger<ServiceBusEventBus> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly string _topicName = "empower_blog_event_bus";
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;
        private readonly string AUTOFAC_SCOPE_NAME = "empower_blog_event_bus";
        private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";

        public ServiceBusEventBus(IServiceBusPersisterConnection serviceBusPersisterConnection,
            ILogger<ServiceBusEventBus> logger,
            IEventBusSubscriptionsManager subsManager,
            IConfiguration configuration)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _sender = serviceBusPersisterConnection.TopicClient.CreateSender(_topicName);
            ServiceBusProcessorOptions options = new ServiceBusProcessorOptions { MaxConcurrentCalls = 10, AutoCompleteMessages = false };
            _processor = serviceBusPersisterConnection.TopicClient.CreateProcessor(_topicName, configuration["EventBus:SubscriptionClientName"], options);
        }

        public void Publish(IntegrationEvent @event)
        {
            var eventName = @event.GetType().Name.Replace(INTEGRATION_EVENT_SUFFIX, "");
            var jsonMessage = JsonSerializer.Serialize(@event, @event.GetType());
            var body = Encoding.UTF8.GetBytes(jsonMessage);

            var message = new ServiceBusMessage
            {
                MessageId = Guid.NewGuid().ToString(),
                Body = new BinaryData(body),
                Subject = eventName,
            };

            _sender.SendMessageAsync(message)
                .GetAwaiter()
                .GetResult();
        }

        public void Subscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, typeof(TH).Name);

            _subsManager.AddSubscription<T, TH>();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            var eventName = typeof(T).Name.Replace(INTEGRATION_EVENT_SUFFIX, "");

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);

            _subsManager.RemoveSubscription<T, TH>();
        }

        public async ValueTask DisposeAsync()
        {
            _subsManager.Clear();
            await _processor.CloseAsync();
        }
    }
}