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

        public Task CreateBlogAsync(BlogDto blog)
        {
            throw new NotImplementedException();
        }

        public Task DeleteBlogAsync(Guid blogId)
        {
            throw new NotImplementedException();
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
