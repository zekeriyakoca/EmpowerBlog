using EmpowerBlog.Web.API.Dtos;
using EmpowerBlog.Web.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web.Resource;

namespace EmpowerBlog.Web.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    //[RequiredScope(RequiredScopesConfigurationKey = "AzureAd:Scopes")]
    public class BlogController : ControllerBase
    {

        private readonly ILogger<BlogController> _logger;
        private readonly IBlogService blogService;
        private readonly IReviewService reviewService;

        public BlogController(ILogger<BlogController> logger, IBlogService blogService, IReviewService reviewService)
        {
            _logger = logger;
            this.blogService = blogService;
            this.reviewService = reviewService;
        }

        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetBlog([FromRoute] Guid blogId)
        {
            var a = User;
            var blog = await blogService.GetBlogAsync(blogId);
            if (blog == null)
            {
                return NotFound();
            }
            var blogDetail = new BlogDetailDto(blog);
            blogDetail.Reviews = await reviewService.GetReviewsAsync(blogId);

            return Ok(blogDetail);
        }

        [HttpGet("Test")]
        public async Task<IActionResult> Test()
        {
            var newBlog = new BlogDto()
            {
                Id = Guid.NewGuid(),
                Name = "Test1",
                Description = "Test Description 1"
            };

            await blogService.CreateBlogAsync(newBlog);
            await blogService.DeleteBlogAsync(newBlog.Id);

            return Ok();
        }
    }
}