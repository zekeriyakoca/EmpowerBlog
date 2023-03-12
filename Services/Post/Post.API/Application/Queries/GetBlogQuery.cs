using EmpowerBlog.Services.Post.API.Dtos;
using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace EmpowerBlog.Services.Post.API.Application.Commands
{
    // TODO : convert return type to paginated list
    public class GetBlogQuery : IQuery<BlogDto>
    {
        public GetBlogQuery(Guid blogId)
        {
            BlogId = blogId;
        }
        public Guid BlogId { get; set; }
    }
    public class GetBlogQueryHandler : QueryHandler<GetBlogQuery, BlogDto>
    {

        public GetBlogQueryHandler(PostQueryContext context, ILogger<GetBlogQueryHandler> logger) : base(context, logger)
        { }

        public override async Task<BlogDto> Action(GetBlogQuery query)
        {
            ArgumentNullException.ThrowIfNull(query, nameof(query));

            var blog = await Context.Blogs.
                FromSql($"SELECT * FROM Blogs WHERE [Id] = {query.BlogId}")
                .SingleOrDefaultAsync();

            return blog.ToBlogDto();
        }
    }
}
