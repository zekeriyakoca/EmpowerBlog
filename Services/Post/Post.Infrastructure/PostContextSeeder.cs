using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;

namespace EmpowerBlog.Services.Post.API.Infrastructure
{
    public class PostContextSeeder
    {
        private readonly PostContext context;

        public PostContextSeeder(PostContext context)
        {
            this.context = context;
        }

        public async Task SeedAsync()
        {
            if (!context.Blogs.Any())
            {
                context.Blogs.Add(new Blog
                {
                    Id = new Guid("28013d52-5436-4c96-9d0d-8612d872546f"),
                    Name = "Sample Blog",
                    Description = "Sample Blog Description"
                });
                await context.SaveChangesAsync();

            }


        }
    }
}
