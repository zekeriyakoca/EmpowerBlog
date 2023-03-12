using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using EmpowerBlog.Services.Post.Domain;
using EmpowerBlog.Services.Post.Domain.Events;
using EventBus.Interfaces;
using EmpowerBlog.Services.Post.API.Application.IntegrationEvents;

namespace EmpowerBlog.Services.Post.API.Application.Commands
{
    public class DeleteBlogCommand : ICommand<bool>
    {
        public Guid Id { get; set; }

    }
    public class DeleteBlogCommandHandler : CommandHandler<DeleteBlogCommand, bool>
    {
        private readonly PostContext context;
        private readonly IEventBus eventBus;

        public DeleteBlogCommandHandler(PostContext context, IEventBus eventBus)
        {
            this.context = context;
            this.eventBus = eventBus;
        }

        public override async Task<bool> Action(DeleteBlogCommand query)
        {
            var current = await context.Blogs.FindAsync(query.Id);
            if (current == null)
                throw new DomainException($"Blog couldn't be found! BlogId = {query.Id}");

            current.AddDomainEvent(new BlogDeletedDomainEvent());

            context.Blogs.Remove(current);

            var isSuccess = await context.SaveChangesAsync() > 0;

            if (isSuccess)
            {
                eventBus.Publish(new BlogDeletedIntegrationEvent { BlogId = query.Id });
            }
            return isSuccess;
        }
    }
}
