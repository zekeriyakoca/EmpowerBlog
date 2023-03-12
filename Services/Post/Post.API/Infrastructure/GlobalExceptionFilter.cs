using Microsoft.AspNetCore.Mvc.Filters;

namespace EmpowerBlog.Services.Post.API.Infrastructure
{
    public class GlobalExceptionFilter : IExceptionFilter
    {

        public void OnException(ExceptionContext context)
        {
            throw new NotImplementedException();
        }
    }
}
