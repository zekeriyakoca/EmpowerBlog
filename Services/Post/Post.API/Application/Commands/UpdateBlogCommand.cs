using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using EmpowerBlog.Services.Post.Domain;

namespace EmpowerBlog.Services.Post.API.Application.Commands
{
    public class UpdateBlogCommand : ICommand<bool>
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

    }
    public class UpdateBlogCommanddHandler : CommandHandler<UpdateBlogCommand, bool>
    {
        private readonly PostContext context;

        public UpdateBlogCommanddHandler(PostContext context)
        {
            this.context = context;
        }

        public override async Task<bool> Action(UpdateBlogCommand query)
        {
            var current = await context.Blogs.FindAsync(query.Id);
            if (current == null)
                throw new DomainException($"Blog couldn't be found! BlogId = {query.Id}");

            current.Name = query.Name;
            current.Description = query.Description;

            await context.SaveChangesAsync();
            return true;
        }
    }
}
