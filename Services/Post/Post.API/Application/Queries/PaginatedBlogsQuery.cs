using EmpowerBlog.Services.Post.API.Dtos;
using EmpowerBlog.Services.Post.API.Infrastructure;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using Microsoft.EntityFrameworkCore;

namespace EmpowerBlog.Services.Post.API.Application.Commands
{
    // TODO : convert return type to paginated list
    public class PaginatedBlogsQuery : IQuery<IEnumerable<BlogDto>>
    {
        public int TakeCount { get; set; } = 10;
        public int SkipCount { get; set; } = 0;
    }
    public class PaginatedBlogsQueryHandler : QueryHandler<PaginatedBlogsQuery, IEnumerable<BlogDto>>
    {

        public PaginatedBlogsQueryHandler(PostQueryContext context, ILogger<PaginatedBlogsQueryHandler> logger) : base(context, logger)
        { }

        public override async Task<IEnumerable<BlogDto>> Action(PaginatedBlogsQuery query)
        {
            ArgumentNullException.ThrowIfNull(query, nameof(query));

            var blogs = await Context.Blogs.
               FromSql($"""
                SELECT * FROM Blogs
                ORDER BY Id
                OFFSET {query.SkipCount} ROWS
                FETCH NEXT {query.TakeCount} ROWS ONLY;
                """)
               .ToListAsync();

            return blogs.Select(b => b.ToBlogDto()).ToList();
        }
    }
}
