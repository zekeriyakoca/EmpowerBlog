using Microsoft.EntityFrameworkCore;
using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;
using EmpowerBlog.Services.Post.Domain.Common;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using EmpowerBlog.Services.Post.Domain;
using Microsoft.AspNetCore.Http;

namespace EmpowerBlog.Services.Post.API.Infrastructure
{
    public class PostContext : DbContext
    {
        private readonly DbContextOptions<PostContext> options;
        private readonly IHttpContextAccessor httpContextAccessor;

        public PostContext(DbContextOptions<PostContext> options, IHttpContextAccessor httpContextAccessor) : base(options)
        {
            this.options = options;
            this.httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            OnSaveChangesAsync().Wait();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await OnSaveChangesAsync();
            return await base.SaveChangesAsync(cancellationToken);
        }


        private async Task OnSaveChangesAsync()
        {
            var domainEntities = ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents != null && x.Entity.DomainEvents.Any());

            var domainEvents = domainEntities.SelectMany(x => x.Entity.DomainEvents).ToList();

            await HandleDomainEvents(domainEntities, domainEvents);

        }

        private async Task HandleDomainEvents(IEnumerable<EntityEntry<Entity>> domainEntities, List<IDomainEvent> domainEvents)
        {
            if (domainEvents.Any())
            {
                domainEntities.ToList()
                    .ForEach(e => e.Entity.ClearDomainEvents());

                foreach (var e in domainEvents)
                {
                    var handlerType = typeof(IHandler<>).MakeGenericType(e.GetType());

                    if (httpContextAccessor.HttpContext == null)
                    {
                        // TODO : Log warninig saying "entities with domain entities called outside of request lifecycle"
                        return;
                    }

                    var handler = httpContextAccessor.HttpContext.RequestServices.GetService(handlerType);

                    if (handler != null)
                    {
                        var handlerMethod = handlerType.GetMethod("Handle");
                        await (Task)handlerMethod.Invoke(handler, new object[] { e });
                    }
                }

            }
        }
    }
}
