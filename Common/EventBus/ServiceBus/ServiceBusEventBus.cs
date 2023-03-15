using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using EventBus.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace EventBus.ServiceBus
{
    public class ServiceBusEventBus : IEventBus
    {
        private readonly IServiceBusPersisterConnection _serviceBusPersisterConnection;
        private readonly ILogger<ServiceBusEventBus> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly string _topicName = "empower_blog_event_bus";
        private readonly string _subscriptionName;
        private readonly ServiceBusSender _sender;
        private readonly ServiceBusProcessor _processor;
        private readonly string AUTOFAC_SCOPE_NAME = "empower_blog_event_bus";
        private const string INTEGRATION_EVENT_SUFFIX = "IntegrationEvent";

        public ServiceBusEventBus(IServiceBusPersisterConnection serviceBusPersisterConnection,
            ILogger<ServiceBusEventBus> logger,
            IEventBusSubscriptionsManager subsManager,
            IConfiguration configuration,
            IServiceProvider serviceProvider)
        {
            _subscriptionName = configuration["EventBus:SubscriptionClientName"];
            _serviceBusPersisterConnection = serviceBusPersisterConnection;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _serviceProvider = serviceProvider;
            _sender = _serviceBusPersisterConnection.TopicClient.CreateSender(_topicName);
            ServiceBusProcessorOptions options = new ServiceBusProcessorOptions { MaxConcurrentCalls = 10, AutoCompleteMessages = false };
            _processor = _serviceBusPersisterConnection.TopicClient.CreateProcessor(_topicName, _subscriptionName, options);

            EnsureTopicAndSubscriptionExistence().GetAwaiter().GetResult();
            RegisterSubscriptionClientMessageHandlerAsync().GetAwaiter().GetResult();
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

        private async Task EnsureTopicAndSubscriptionExistence()
        {
            if (!await _serviceBusPersisterConnection.AdministrationClient.TopicExistsAsync(_topicName))
                await _serviceBusPersisterConnection.AdministrationClient.CreateTopicAsync(_topicName);
            if (!await _serviceBusPersisterConnection.AdministrationClient.SubscriptionExistsAsync(_topicName, _subscriptionName))
                await _serviceBusPersisterConnection.AdministrationClient.CreateSubscriptionAsync(_topicName, _subscriptionName);
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

        private async Task RegisterSubscriptionClientMessageHandlerAsync()
        {
            _processor.ProcessMessageAsync +=
                async (args) =>
                {
                    var eventName = $"{args.Message.Subject}{INTEGRATION_EVENT_SUFFIX}";
                    string messageData = args.Message.Body.ToString();

                    if (await ProcessEventAsync(eventName, messageData))
                    {
                        await args.CompleteMessageAsync(args.Message);
                    }
                };

            _processor.ProcessErrorAsync += (errorArgs) =>
            {
                _logger.LogError(ex, "ERROR handling message: {ExceptionMessage} - Context: {@ExceptionContext}", ex.Message, context);
                return Task.CompletedTask;
            };
            await _processor.StartProcessingAsync();
        }

        private async Task<bool> ProcessEventAsync(string eventName, string message)
        {
            using var scope = _serviceProvider.CreateScope();
            var eventHandlers = _subsManager.GetHandlersForEvent(eventName);
            foreach (var eventHandlerType in eventHandlers)
            {
                var eventType = _subsManager.GetEventTypeByName(eventName);
                var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                var handler =  scope.ServiceProvider.GetService(concreteType);
                if (handler == null) continue;
                var integrationEvent = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
                await (Task)concreteType.GetMethod("Handle").Invoke(handler, new object[] { integrationEvent });
            }
            return true;
        }

        public async ValueTask DisposeAsync()
        {
            _subsManager.Clear();
            await _processor.CloseAsync();
        }
    }
}