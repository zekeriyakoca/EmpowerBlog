using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;

namespace EmpowerBlog.Services.Post.API.Dtos
{
    // TODO : Replace with AutoMapper
    public static class BlogBinderExtensions
    {
        public static BlogDto ToBlogDto(this Blog comment)
        {
            if (comment == null)
                return default;

            return new BlogDto
            {
                Id= comment.Id.ToString(),
                Name = comment.Name,
                Description = comment.Description,
            };
        }
    }
}
