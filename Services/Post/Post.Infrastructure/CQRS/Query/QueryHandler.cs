using EmpowerBlog.Services.Post.API.Infrastructure;
using Microsoft.Extensions.Logging;

namespace EmpowerBlog.Services.Post.infrastructure.CQRS
{
    public interface IQueryHandler<TQuery, TResult>
    {
        public Task<TResult> Handle(TQuery query);
    }

    public abstract class QueryHandler<TQuery, TResult> : IQueryHandler<TQuery, TResult>
    {
        public ILogger<QueryHandler<TQuery, TResult>> Logger { get; }
        public PostQueryContext Context { get; }

        public QueryHandler(PostQueryContext context, ILogger<QueryHandler<TQuery, TResult>> logger)
        {
            Logger = logger;
            Context = context;
        }

        public async Task<TResult> Handle(TQuery query)
        {
            try
            {
                var result = await Action(query);

                return result;
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error occured during execution of query [{query.GetType()}:{nameof(query)}]. Error message : {ex.Message}");
                throw;
            }
        }


        public abstract Task<TResult> Action(TQuery query);
    }
}
