using Microsoft.EntityFrameworkCore;
using EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels;

namespace EmpowerBlog.Services.Post.API.Infrastructure
{
    public class PostQueryContext : DbContext
    {
        private readonly DbContextOptions<PostQueryContext> options;

        public PostQueryContext(DbContextOptions<PostQueryContext> options) : base(options)
        {
            this.ChangeTracker.AutoDetectChangesEnabled = false;
            this.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            this.options = options;
        }

        public DbSet<Blog> Blogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
        }
    }
}
