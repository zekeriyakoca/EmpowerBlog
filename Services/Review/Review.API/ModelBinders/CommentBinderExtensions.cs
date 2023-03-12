using EmpowerBlog.Services.Review.API.Dtos;

namespace EmpowerBlog.Services.Review.API.Entities
{
    public static class CommentBinderExtensions
    {
        public static CommentDto ToCommentDto(this Comment comment)
        {
            return new CommentDto
            {
                Id = comment.Id,
                Text = comment.Text,
            };
        }
    }
}
