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
            if (context.Commnets.Any())
            {
                // TODO : Add default comments
            }
        }
    }
}
