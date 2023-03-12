using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmpowerBlog.Services.Post.Domain
{
    public interface IHandler<T> where T : IDomainEvent
    {
        Task Handle(T eventParams);
    }
}
