using EmpowerBlog.Services.Post.API.Application.Commands;
using EmpowerBlog.Services.Post.infrastructure.CQRS;
using Microsoft.AspNetCore.Mvc;

namespace EmpowerBlog.Services.Post.API.Controllers
{
    [ApiController]
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
        public IActionResult GetBlog([FromRoute] Guid blogId)
        {
            var blog = queryExecuter.ExecuteAsync(new GetBlogQuery(blogId));

            if (blog == default)
                return NotFound();
            return Ok(blog);
        }

        [HttpPost("paginated")]
        public async Task<IActionResult> GetAllBlogs([FromBody] PaginatedBlogsQuery query)
        {
            var blogs = await queryExecuter.ExecuteAsync(query);
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
        public IActionResult UpdateBlog([FromQuery] Guid blogId)
        {
            return Ok();
        }
    }
}