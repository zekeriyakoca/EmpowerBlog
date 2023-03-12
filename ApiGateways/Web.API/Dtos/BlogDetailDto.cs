namespace EmpowerBlog.Web.API.Dtos
{
    public class BlogDetailDto
    {
        public BlogDetailDto()  { }

        public BlogDetailDto(BlogDto blog)
        {
            Id = blog.Id;
            Title= blog.Name;
            Description= blog.Description;
        }
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public IEnumerable<ReviewDto> Reviews { get; set; }
    }
}
