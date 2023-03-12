using EventBus;

namespace EmpowerBlog.Services.Post.API.Application.IntegrationEvents
{
    public record BlogDeletedIntegrationEvent : IntegrationEvent
    {
        public Guid BlogId { get; set; }
    }
}
