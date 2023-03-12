using EmpowerBlog.Services.Post.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpowerBlog.Services.Post.Domain.Aggregates.BlogModels
{
    public class Blog : Entity, IAggregateRoot
    {
        [MaxLength(200)]
        public string Name { get; set; }

        public string Description { get; set; }
    }
}
