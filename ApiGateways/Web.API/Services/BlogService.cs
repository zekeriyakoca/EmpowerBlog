using EmpowerBlog.Web.API.Dtos;
using EmpowerBlog.Web.API.Services.Interfaces;

namespace EmpowerBlog.Web.API.Services
{
    public class BlogService : IBlogService
    {
        private readonly HttpClient client;

        public BlogService(HttpClient client)
        {
            this.client = client;
        }

        public async Task CreateBlogAsync(BlogDto blog)
        {
            var result = await client.PostAsJsonAsync($"/api/post", blog);
            result.EnsureSuccessStatusCode();
        }

        public async Task DeleteBlogAsync(Guid blogId)
        {
            var result = await client.DeleteAsync($"/api/post/{blogId}");
            result.EnsureSuccessStatusCode();
        }

        public async Task<BlogDto> GetBlogAsync(Guid blogId)
        {
            var result = await client.GetAsync($"/api/post/{blogId}");
            var blog = await result.Content.ReadFromJsonAsync<BlogDto>();
            return blog;
        }

        public Task<IEnumerable<BlogDto>> GetBlogsAsync(int page, int take)
        {
            throw new NotImplementedException();
        }

        public Task UpdateBlogAsync(BlogDto blog)
        {
            throw new NotImplementedException();
        }
    }
}
