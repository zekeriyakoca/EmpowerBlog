using EmpowerBlog.Services.Post.API.Application.Commands;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace EmpowerBlog.Services.Post.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class PostController : ControllerBase
    {
        private readonly ILogger<PostController> _logger;
        private readonly IQueryExecuter queryExecuter;
        private readonly ICommandExecuter commandExecuter;

        public PostController(ILogger<PostController> logger, IQueryExecuter queryExecuter, ICommandExecuter commandExecuter)
        {
            _logger = logger;
            this.queryExecuter = queryExecuter;
            this.commandExecuter = commandExecuter;
        }

        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetBlog([FromRoute] Guid blogId)
        {
            var blog = await queryExecuter.ExecuteAsync(new GetBlogQuery(blogId));

            if (blog == default)
                return NotFound();
            return Ok(blog);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBlogs([FromQuery] int page = 0, [FromQuery] int take = 10)
        {
            var blogs = await queryExecuter.ExecuteAsync(new PaginatedBlogsQuery
            {
                SkipCount = page * take,
                TakeCount = take
            });
            return Ok(blogs);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBlog([FromBody] InsertBlogCommand command)
        {
            if (command == null)
                return BadRequest();

            var isSuccess = await commandExecuter.ExecuteAsync(command);
            if (isSuccess)
                return Ok();

            return BadRequest("Blog couldn't be cretated! Please check your parameters");
        }

        [HttpPut("{blogId}")]
        public IActionResult UpdateBlog([FromRoute] Guid blogId)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("{blogId}")]
        public async Task<IActionResult> DeleteBlog([FromRoute] Guid blogId)
        {
            var isSuccess = await commandExecuter.ExecuteAsync(new DeleteBlogCommand
            {
                Id = blogId
            });

            if (isSuccess)
                return Ok();

            return BadRequest("Blog couldn't be deleted! Please check your parameters");
        }
    }
}