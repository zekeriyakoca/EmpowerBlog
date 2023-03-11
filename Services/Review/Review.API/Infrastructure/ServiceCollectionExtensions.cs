using EmpowerBlog.Services.Review.API.Services;

namespace EmpowerBlog.Services.Review.API.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static void SetupAppServices(this IServiceCollection services)
        {
            services.AddTransient<IReviewService, ReviewService>();

        }
    }
}
