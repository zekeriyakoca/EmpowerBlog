using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;
using EmpowerBlog.Services.Post.infrastructure.CQRS;

namespace EmpowerBlog.Services.Post.API.Application.Commands
{
    public class InsertBlogCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
    public class InsertBlogCommandHandler : CommandHandler<InsertBlogCommand, bool>
    {
        private readonly PostContext context;

        public InsertBlogCommandHandler(PostContext context)
        {
            this.context = context;
        }

        public override async Task<bool> Action(InsertBlogCommand query)
        {
            var newBlog = new Blog
            {
                Id = query.Id,
                Name = query.Name,
                Description = query.Description,
            };

            context.Blogs.Add(newBlog);

            var isSuccess = await context.SaveChangesAsync() > 0;
            return isSuccess;
        }
    }
}
