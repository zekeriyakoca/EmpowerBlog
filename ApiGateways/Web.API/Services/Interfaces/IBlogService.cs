using EmpowerBlog.Web.API.Dtos;

namespace EmpowerBlog.Web.API.Services.Interfaces
{
    public interface IBlogService
    {
        public Task<BlogDto> GetBlogAsync(Guid blogId);
        public Task<IEnumerable<BlogDto>> GetBlogsAsync(int page, int take);
        public Task DeleteBlogAsync(Guid blogId);
        public Task CreateBlogAsync(BlogDto blog);
        public Task UpdateBlogAsync(BlogDto blog);
    }
}
