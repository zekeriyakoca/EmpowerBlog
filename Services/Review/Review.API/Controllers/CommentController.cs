using EmpowerBlog.Services.Review.API.Dtos;
using EmpowerBlog.Services.Review.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmpowerBlog.Services.Review.API.Controllers
{
    [ApiController]
    //[Authorize]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {

        private readonly ILogger<ReviewController> _logger;
        private readonly IReviewService _reviewService;

        public ReviewController(ILogger<ReviewController> logger, IReviewService reviewService)
        {
            _logger = logger;
            _reviewService = reviewService;
        }

        [HttpGet("{blogId}")]
        public ActionResult<IEnumerable<CommentDto>> GetComments([FromRoute]Guid blogId)
        {
            if (blogId == null)
                return BadRequest("{PostId} parameter cannot be null!");

            return Ok(_reviewService.GetCommentsByPost(blogId));
        }
    }
}