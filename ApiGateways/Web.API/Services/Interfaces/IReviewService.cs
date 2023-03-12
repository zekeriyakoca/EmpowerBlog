using EmpowerBlog.Web.API.Dtos;

namespace EmpowerBlog.Web.API.Services.Interfaces
{
    public interface IReviewService
    {
        public Task<IEnumerable<ReviewDto>> GetReviewsAsync(Guid blogId);
    }
}
