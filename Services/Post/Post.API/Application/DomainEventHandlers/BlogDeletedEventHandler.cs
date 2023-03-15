using EmpowerBlog.Services.Post.Domain;
using EmpowerBlog.Services.Post.Domain.Events;

namespace EmpowerBlog.Services.Post.API.Application.DomainEventHandlers
{
    public class BlogDeletedEventHandler : IHandler<BlogDeletedDomainEvent>
    {
        private readonly ILogger<BlogDeletedEventHandler> logger;

        public BlogDeletedEventHandler(ILogger<BlogDeletedEventHandler> logger)
        {
            this.logger = logger;
        }
        public Task Handle(BlogDeletedDomainEvent eventParams)
        {
            logger.LogInformation($"{nameof(BlogDeletedEventHandler)} domain handler triggered", eventParams);
            return Task.CompletedTask;
        }
    }
}
