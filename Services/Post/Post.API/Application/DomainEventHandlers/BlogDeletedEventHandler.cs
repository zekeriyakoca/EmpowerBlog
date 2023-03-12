using EmpowerBlog.Services.Post.Domain;
using EmpowerBlog.Services.Post.Domain.Events;

namespace EmpowerBlog.Services.Post.API.Application.DomainEventHandlers
{
    public class BlogDeletedEventHandler : IHandler<BlogDeletedDomainEvent>
    {
        public Task Handle(BlogDeletedDomainEvent eventParams)
        {
            throw new NotImplementedException();
        }
    }
}
