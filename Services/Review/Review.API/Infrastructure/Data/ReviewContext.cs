﻿using EmpowerBlog.Services.Review.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace EmpowerBlog.Services.Review.API.Infrastructure
{
    public class ReviewContext : DbContext
    {
        private readonly DbContextOptions<ReviewContext> options;

        public ReviewContext(DbContextOptions<ReviewContext> options) : base(options)
        {
            this.options = options;
        }

        public DbSet<Comment> Commnets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
