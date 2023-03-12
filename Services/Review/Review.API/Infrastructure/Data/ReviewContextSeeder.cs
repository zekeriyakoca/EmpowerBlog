namespace EmpowerBlog.Services.Review.API.Infrastructure
{
    public class ReviewContextSeeder
    {
        private readonly ReviewContext context;

        public ReviewContextSeeder(ReviewContext context)
        {
            this.context = context;
        }

        public async Task SeedAsync()
        {
            if (!context.Commnets.Any())
            {
                context.Commnets.Add(new Entities.Comment
                {
                    Id = new Guid("28013d52-5436-4c96-9d0d-8612d8725111"),
                    BlogId = new Guid("28013d52-5436-4c96-9d0d-8612d8725000"),
                });
                await context.SaveChangesAsync();
            }
        }
    }
}
