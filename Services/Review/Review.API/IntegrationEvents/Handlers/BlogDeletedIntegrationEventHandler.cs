using EventBus.Interfaces;

namespace EmpowerBlog.Services.Review.API.IntegrationEvents.Handlers
{
    public class BlogDeletedIntegrationEventHandler : IIntegrationEventHandler<BlogDeletedIntegrationEvent>
    {
        public Task Handle(BlogDeletedIntegrationEvent @event)
        {
            throw new NotImplementedException();
        }
    }
}
