using EmpowerBlog.Services.Review.API.Dtos;
using EmpowerBlog.Services.Review.API.Entities;
using EmpowerBlog.Services.Review.API.Infrastructure;

namespace EmpowerBlog.Services.Review.API.Services
{
    public interface IReviewService
    {
        IEnumerable<CommentDto> GetCommentsByPost(Guid postId);
    }

    public class ReviewService : IReviewService
    {
        private readonly ReviewContext context;

        public ReviewService(ReviewContext context)
        {
            this.context = context;
        }

        public IEnumerable<CommentDto> GetCommentsByPost(Guid postId)
        {
            var comments = context.Commnets.Where(c => c.BlogId == postId).AsEnumerable();
            return comments.Select(c => c.ToCommentDto());
        }


    }

}
