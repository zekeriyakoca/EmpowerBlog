using EmpowerBlog.Web.API.Dtos;
using EmpowerBlog.Web.API.Services.Interfaces;

namespace EmpowerBlog.Web.API.Services
{
    public class ReviewService : IReviewService
    {
        private readonly HttpClient client;

        public ReviewService(HttpClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<ReviewDto>> GetReviewsAsync(Guid blogId)
        {
            var result = await client.GetAsync($"/api/review/{blogId}");
            var blog = await result.Content.ReadFromJsonAsync<IEnumerable<ReviewDto>>();
            return blog;
        }
    }
}
